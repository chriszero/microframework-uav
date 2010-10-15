using System;

using Extensions;

namespace QuadroLib.Control {
    public class PidControl : IControl {
        //scaled, tweaked parameters we'll actually be using
        private double accError; // * the (I)ntegral term is based on the sum of error over
        //   time.  this variable keeps track of that
        private double bias; // * the base output from which the PID operates
        private double P_Param;
        private double I_Param;
        private double D_Param;
        private bool inAuto; // * Flag letting us know if we are in Automatic or not
        private double inMin, inSpan; // * input and output limits, and spans.  used convert
        private bool justCalced; // * flag gets set for one cycle after the pid calculates
        private double kc; // * (P)roportional Tuning Parameter
        private double lastInput; // * we need to remember the last Input Value so we can compute
        private double lastOutput; // * remembering the last output is used to prevent

        private DateTime nextCompTime; // * Helps us figure out when the PID Calculation needs to
        //   be performed next
        //   to determine when to compute next
        private double outMin, outSpan; //   real world numbers into percent span, with which
        private double taud; // * (D)erivative Tuning Parameter
        private double taur; // * (I)ntegral Tuning Parameter
        private TimeSpan tSample; // * the frequency, in milliseconds, with which we want the
        // private bool UsingFeedForward; // * internal flag that tells us if we're using FeedForward or not
        //   the PID algorithm is more comfortable.

        /// <summary>
        /// Standart constructor used by most users.  the parameters specified are those for
        /// for which we can't set up reliable defaults, so we need to have the user
        /// set them.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="setPoint"></param>
        /// <param name="Kc"></param>
        /// <param name="TauI"></param>
        /// <param name="TauD"></param>
        public PidControl(double Kc, double TauI, double TauD) {

            this.inSpan = 1023;
            this.SetInputLimits(0, 1023); //default the limits to the
            this.outSpan = 255;
            this.SetOutputLimits(0, 255); //full ranges of the I/O

            this.tSample = new TimeSpan(0, 0, 0, 1); //default Controller Sample Time is 1 second

            this.SetTunings(Kc, TauI, TauD);

            this.nextCompTime = DateTime.Now;
            this.inAuto = true;

            this.Reset();
        }

        /// <summary>
        /// I don't see this function being called all that much (other than from the
        ///  constructor.)  it needs to be here so we can tell the controller what it's
        ///  input limits are, and in most cases the 0-1023 default should be fine.  if
        ///  there's an application where the signal being fed to the controller is
        ///  outside that range, well, then this function's here for you.
        /// </summary>
        /// <param name="INMin"></param>
        /// <param name="INMax"></param>
        public void SetInputLimits(double INMin, double INMax) {
            //after verifying that mins are smaller than maxes, set the values
            if (INMin >= INMax) return;

            //rescale the working variables to reflect the changes
            this.lastInput = (this.lastInput) * (INMax - INMin) / (this.inSpan);
            this.accError *= (INMax - INMin) / (this.inSpan);

            //make sure the working variables are 
            //within the new limits
            if (this.lastInput > 1) this.lastInput = 1;
            else if (this.lastInput < 0) this.lastInput = 0;


            this.inMin = INMin;
            this.inSpan = INMax - INMin;
        }

        /// <summary>
        /// This function will be used far more often than SetInputLimits.  while
        ///  the input to the controller will generally be in the 0-1023 range (which is
        ///  the default already,)  the output will be a little different.  maybe they'll
        ///  be doing a time window and will need 0-8000 or something.  or maybe they'll
        ///  want to clamp it from 0-125.  who knows.  at any rate, that can all be done
        ///  here.
        /// </summary>
        /// <param name="OUTMin"></param>
        /// <param name="OUTMax"></param>
        public void SetOutputLimits(double OUTMin, double OUTMax) {
            //after verifying that mins are smaller than maxes, set the values
            if (OUTMin >= OUTMax) return;

            //rescale the working variables to reflect the changes
            this.lastOutput = (this.lastOutput) * (OUTMax - OUTMin) / (this.outSpan);

            //make sure the working variables are 
            //within the new limits
            if (this.lastOutput > 1) this.lastOutput = 1;
            else if (this.lastOutput < 0) this.lastOutput = 0;

            this.outMin = OUTMin;
            this.outSpan = OUTMax - OUTMin;
        }

        /// <summary>
        /// This function allows the controller's dynamic performance to be adjusted. 
        /// it's called automatically from the constructor, but tunings can also
        /// be adjusted on the fly during normal operation
        /// </summary>
        /// <param name="Kc"></param>
        /// <param name="TauI"></param>
        /// <param name="TauD"></param>
        public void SetTunings(double Kc, double TauI, double TauD) {
            //verify that the tunings make sense
            if (Kc == 0.0 || TauI < 0.0 || TauD < 0.0) return;

            //we're going to do some funky things to the input numbers so all
            //our math works out, but we want to store the numbers intact
            //so we can return them to the user when asked.
            this.P_Param = Kc;
            this.I_Param = TauI;
            this.D_Param = TauD;

            //convert Reset Time into Reset Rate, and compensate for Calculation frequency
            //double tSampleInSec = (this.tSample.TotalMilliseconds() / 1000.0);
            double tSampleInSec = (this.tSample.TotalMicroseconds());
            double tempTauR;
            if (TauI == 0.0)
                tempTauR = 0.0;
            else
                tempTauR = (1.0 / TauI) * tSampleInSec;

            if (this.inAuto) {
                //if we're in auto, and we just change the tunings, the integral term 
                //will become very, very, confused (trust me.) to achieve "bumpless
                // transfer" we need to rescale the accumulated error.
                if (tempTauR != 0.0) //(avoid divide by 0)
                    this.accError *= (this.kc * this.taur) / (Kc * tempTauR);
                else
                    this.accError = 0.0;
            }

            this.kc = Kc;
            this.taur = tempTauR;
            this.taud = TauD / tSampleInSec;
        }

        /// <summary>
        ///  does all the things that need to happen to ensure a bumpless transfer
        ///  from manual to automatic mode.  this shouldn't have to be called from the
        ///  outside. In practice though, it is sometimes helpful to start from scratch,
        ///  so it was made publicly available
        /// </summary>
        public void Reset() {
            this.bias = this.outMin / this.outSpan;

            this.lastOutput = this.bias;
            this.lastInput = this.inMin / this.inSpan;

            // - clear any error in the integral
            this.accError = 0;
        }

        /// <summary>
        /// Allows the controller Mode to be set to manual (false) or Automatic (true)
        /// when the transition from manual to auto occurs, the controller is
        /// automatically initialized
        /// </summary>
        public bool Mode {
            set {
                this.inAuto = value;
                if (this.inAuto) {
                    this.Reset();
                }
            }
            get { return this.inAuto; }
        }

        /// <summary>
        /// sets the frequency, in Milliseconds, with which the PID calculation is performed
        /// </summary>
        /// <param name="NewSampleTime"></param>
        public void SetSampleTime(int NewSampleTime) {
            if (NewSampleTime > 0) {
                //convert the time-based tunings to reflect this change
                this.taur *= ((double)NewSampleTime) / (this.tSample.TotalMilliseconds());
                this.accError *= (this.tSample.TotalMilliseconds()) / ((double)NewSampleTime);
                this.taud *= ((double)NewSampleTime) / (this.tSample.TotalMilliseconds());
                this.tSample = new TimeSpan(0, 0, 0, 0, NewSampleTime);
            }
        }

        public void Compute(double setPoint, double input, out double output) {
            Compute(setPoint, input, out output);
        }

        /// <summary>
        /// This, as they say, is where the magic happens.  this function should be called
        ///   every time "void loop()" executes.  the function will decide for itself whether a new
        ///   pid Output needs to be computed
        ///
        ///  Some notes for people familiar with the nuts and bolts of PID control:
        ///  - I used the Ideal form of the PID equation.  mainly because I like IMC
        ///    tunings.  lock in the I and D, and then just vary P to get more 
        ///    aggressive or conservative
        ///
        ///  - While this controller presented to the outside world as being a Reset Time
        ///    controller, when the user enters their tunings the I term is converted to
        ///    Reset Rate.  I did this merely to avoid the div0 error when the user wants
        ///    to turn Integral action off.
        ///    
        ///  - Derivative on Measurement is being used instead of Derivative on Error.  The
        ///    performance is identical, with one notable exception.  DonE causes a kick in
        ///    the controller output whenever there's a setpoint change. DonM does not.
        ///
        ///  If none of the above made sense to you, and you would like it to, go to:
        ///  http://www.controlguru.com .  Dr. Cooper was my controls professor, and is
        ///  gifted at concisely and clearly explaining PID control
        /// </summary>
        public void Compute(double setPoint, double input, out double output, double myBias = double.MinValue) {
            this.justCalced = false;
            output = 0;
            if (!this.inAuto)
                return; //if we're in manual just leave;

            DateTime now = DateTime.Now;

            //millis() wraps around to 0 at some point.  depending on the version of the 
            //Arduino Program you are using, it could be in 9 hours or 50 days.
            //this is not currently addressed by this algorithm.


            //...Perform PID Computations if it's time...
            if (now >= this.nextCompTime) {
                //pull in the input and setpoint, and scale them into percent span
                double scaledInput = (input - this.inMin) / this.inSpan;
                if (scaledInput > 1.0) scaledInput = 1.0;
                else if (scaledInput < 0.0) scaledInput = 0.0;

                double scaledSP = (setPoint - this.inMin) / this.inSpan;
                if (scaledSP > 1.0) scaledSP = 1;
                else if (scaledSP < 0.0) scaledSP = 0;

                //compute the error
                double err = scaledSP - scaledInput;

                // check and see if the output is pegged at a limit and only 
                // integrate if it is not. (this is to prevent reset-windup)
                if (!(this.lastOutput >= 1 && err > 0) && !(this.lastOutput <= 0 && err < 0)) {
                    this.accError = this.accError + err;
                }

                // compute the current slope of the input signal
                double dMeas = (scaledInput - this.lastInput);
                // we'll assume that dTime (the denominator) is 1 second. 
                // if it isn't, the taud term will have been adjusted 
                // in "SetTunings" to compensate

                //if we're using an external bias (i.e. the user used the 
                //overloaded constructor,) then pull that in now
                if (myBias == double.MinValue) {
                    this.bias = (myBias - this.outMin) / this.outSpan;
                }


                // perform the PID calculation.  
                output = this.bias + this.kc * (err + this.taur * this.accError - this.taud * dMeas);

                //make sure the computed output is within output constraints
                if (output < 0.0) output = 0.0;
                else if (output > 1.0) output = 1.0;

                this.lastOutput = output; // remember this output for the windup
                // check next time		
                this.lastInput = scaledInput; // remember the Input for the derivative
                // calculation next time

                //scale the output from percent span back out to a real world number
                output = ((output * this.outSpan) + this.outMin);

                this.nextCompTime += this.tSample; // determine the next time the computation
                if (this.nextCompTime < now) this.nextCompTime = now + this.tSample; // should be performed	

                this.justCalced = true;
                //set the flag that will tell the outside world that the output was just computed
            }
        }

        public bool JustCalced {
            get { return this.justCalced; }
        }

        public double INMin {
            get { return this.inMin; }
        }

        public double INMax {
            get { return this.inMin + this.inSpan; }
        }

        public double OUTMin {
            get { return this.outMin; }
        }

        public double OUTMax {
            get { return this.outMin + this.outSpan; }
        }

        public TimeSpan SampleTime {
            get { return this.tSample; }
        }

        public double PParam {
            get { return this.P_Param; }
        }

        public double IParam {
            get { return this.I_Param; }
        }

        public double DParam {
            get { return this.D_Param; }
        }
    }
}