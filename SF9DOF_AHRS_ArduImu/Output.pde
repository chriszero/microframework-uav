
void printdata(void)
{
  #if PRINT_BINARY != 1
      Serial.print("!");

      #if PRINT_EULER == 1
      Serial.print("ANG:");
      Serial.print(ToDeg(roll));
      Serial.print(",");
      Serial.print(ToDeg(pitch));
      Serial.print(",");
      Serial.print(ToDeg(yaw));
      #endif      
      #if PRINT_ANALOGS==1
      Serial.print(",AN:");
      Serial.print(AN[sensors[0]]);  //(int)read_adc(0)
      Serial.print(",");
      Serial.print(AN[sensors[1]]);
      Serial.print(",");
      Serial.print(AN[sensors[2]]);  
      Serial.print(",");
      Serial.print(ACC[0]);
      Serial.print (",");
      Serial.print(ACC[1]);
      Serial.print (",");
      Serial.print(ACC[2]);
      Serial.print(",");
      Serial.print(magnetom_x);
      Serial.print (",");
      Serial.print(magnetom_y);
      Serial.print (",");
      Serial.print(magnetom_z);      
      #endif
      /*#if PRINT_DCM == 1
      Serial.print (",DCM:");
      Serial.print(convert_to_dec(DCM_Matrix[0][0]));
      Serial.print (",");
      Serial.print(convert_to_dec(DCM_Matrix[0][1]));
      Serial.print (",");
      Serial.print(convert_to_dec(DCM_Matrix[0][2]));
      Serial.print (",");
      Serial.print(convert_to_dec(DCM_Matrix[1][0]));
      Serial.print (",");
      Serial.print(convert_to_dec(DCM_Matrix[1][1]));
      Serial.print (",");
      Serial.print(convert_to_dec(DCM_Matrix[1][2]));
      Serial.print (",");
      Serial.print(convert_to_dec(DCM_Matrix[2][0]));
      Serial.print (",");
      Serial.print(convert_to_dec(DCM_Matrix[2][1]));
      Serial.print (",");
      Serial.print(convert_to_dec(DCM_Matrix[2][2]));
      #endif*/
      Serial.println();
  #else
      //  This section outputs a binary data message
      //  Conforms to new binary message standard (12/31/09)
      byte IMU_buffer[22];
      int tempint;
      long templong;
      int ck;
      byte IMU_ck_a=0;
      byte IMU_ck_b=0;
      
      Serial.print("DIYd");  // This is the message preamble
      IMU_buffer[0]=0x06; // Payload
      ck=6;
      IMU_buffer[1] = 0x02; // MessageID

      tempint=ToDeg(roll)*100;  //Roll (degrees) * 100 in 2 bytes
      IMU_buffer[2]=tempint&0xff;
      IMU_buffer[3]=(tempint>>8)&0xff;
      
      tempint=ToDeg(pitch)*100;   //Pitch (degrees) * 100 in 2 bytes
      IMU_buffer[4]=tempint&0xff;
      IMU_buffer[5]=(tempint>>8)&0xff;
      
      templong=ToDeg(yaw)*100;  //Yaw (degrees) * 100 in 2 bytes
      if(templong>18000) templong -=36000;
      if(templong<-18000) templong +=36000;
      tempint = templong;
      IMU_buffer[6]=tempint&0xff;
      IMU_buffer[7]=(tempint>>8)&0xff;
      
      for (int i=0;i<ck+2;i++) Serial.print (IMU_buffer[i]);
      for (int i=0;i<ck+2;i++) {
         // Serial.print (IMU_buffer[i]);
          IMU_ck_a+=IMU_buffer[i];  //Calculates checksums
          IMU_ck_b+=IMU_ck_a;       
      }
      Serial.print(IMU_ck_a);
      Serial.print(IMU_ck_b);
      
      IMU_ck_a=0; // Zero Checksum
      IMU_ck_b=0;
      Serial.print("DIYd");  // This is the message preamble
      IMU_buffer[0]=12; // Payload
      ck=12;
      IMU_buffer[1] = 0x05;  // Custom MessageID, 0x02 = Euler, 0x03 = GPS, 0x04 = Euler + Airspeed, 0x0a Performance Monitor
      
      tempint=AN[sensors[0]];
      IMU_buffer[2]=tempint&0xff;
      IMU_buffer[3]=(tempint>>8)&0xff;
      
      tempint=AN[sensors[1]];
      IMU_buffer[4]=tempint&0xff;
      IMU_buffer[5]=(tempint>>8)&0xff;
      
      tempint=AN[sensors[2]];
      IMU_buffer[6]=tempint&0xff;
      IMU_buffer[7]=(tempint>>8)&0xff;
      
      tempint=ACC[0];
      IMU_buffer[8]=tempint&0xff;
      IMU_buffer[9]=(tempint>>8)&0xff;
      
      tempint=ACC[1];
      IMU_buffer[10]=tempint&0xff;
      IMU_buffer[11]=(tempint>>8)&0xff;
      
      tempint=ACC[2];
      IMU_buffer[12]=tempint&0xff;
      IMU_buffer[13]=(tempint>>8)&0xff;
      
     // for (int i=0;i<ck+2;i++) Serial.print (IMU_buffer[i]);
      for (int i=0;i<ck+2;i++) {
          Serial.print (IMU_buffer[i]);
          IMU_ck_a+=IMU_buffer[i];  //Calculates checksums
          IMU_ck_b+=IMU_ck_a;       
      }
      Serial.print(IMU_ck_a);
      Serial.print(IMU_ck_b);
  #endif
      
}

long convert_to_dec(float x)
{
  return x*10000000;
}

