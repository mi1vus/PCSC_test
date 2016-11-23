﻿/* Copyright (c) Microsoft Corporation
 * 
 * All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
 * 
 * See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
*/

using System;
using System.IO;

namespace MiFare.PcSc.Iso7816
{
    /// <summary>
    /// Class ApduCommand implments the ISO 7816 apdu commands
    /// </summary>
    public class ApduCommand
    {
        public ApduCommand(byte cla, byte ins, byte p1, byte p2, byte[] commandData, byte? le)
        {
            if (commandData != null && commandData.Length > 254)
            {
                throw new NotImplementedException();
            }
            CLA = cla;
            INS = ins;
            P1 = p1;
            P2 = p2;
            CommandData = commandData;
            Le = le;

            ApduResponseType = typeof(Iso7816.ApduResponse);
        }
        /// <summary>
        /// Class of instructions
        /// </summary>
        public byte CLA { get; set; }
        /// <summary>
        /// Instruction code
        /// </summary>
        public byte INS { get; set; }
        /// <summary>
        /// Instruction parameter 1
        /// </summary>
        public byte P1 { get; set; }
        /// <summary>
        /// Instruction parameter 2
        /// </summary>
        public byte P2 { get; set; }
        /// <summary>
        /// Maximum number of bytes expected in the response ot this command
        /// </summary>
        public byte? Le { get; set; }
        /// <summary>
        /// Contiguous array of bytes representing commands data
        /// </summary>
        public byte[] CommandData { get; set; }
        /// <summary>
        /// Expected response type for this command.
        /// Provides mechanism to bind commands to responses
        /// </summary>
        public Type ApduResponseType { set; get; }
        /// <summary>
        /// Packs the current command into contiguous buffer bytes
        /// </summary>
        /// <returns>
        /// buffer holds the current wire/air format of the command
        /// </returns>
        public byte[] GetBuffer()
        {
            using (var ms = new MemoryStream())
            {
                ms.WriteByte(CLA);
                ms.WriteByte(INS);
                ms.WriteByte(P1);
                ms.WriteByte(P2);

                if (CommandData != null && CommandData.Length > 0)
                {
                    ms.WriteByte((byte)CommandData.Length);
                    ms.Write(CommandData, 0, CommandData.Length);
                }

                if (Le != null)
                {
                    ms.WriteByte((byte)Le);
                }
                return ms.ToArray();
            }

           
        }
        /// <summary>
        /// Helper method to print the command in a readable format
        /// </summary>
        /// <returns>
        /// return string formatted command
        /// </returns>
        public override string ToString()
        {
            return "ApduCommand CLA=" + CLA.ToString("X2") + ",INS=" + INS.ToString("X2") + ",P1=" + P1.ToString("X2") + ",P2=" + P2.ToString("X2") + ((CommandData != null && CommandData.Length > 0) ? (",Data=" + BitConverter.ToString(CommandData).Replace("-", "")) : "");
        }
    }
    /// <summary>
    /// Class ApduResponse implments handler for the ISO 7816 apdu response
    /// </summary>
    public class ApduResponse
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        public ApduResponse() { }
        /// <summary>
        /// methode to extract the response data, status and qualifier
        /// </summary>
        /// <param name="response"></param>
        public virtual void ExtractResponse(byte[] response)
        {
            if (response.Length < 2)
            {
                throw new InvalidOperationException("APDU response must be at least 2 bytes");
            }
            using (var reader = new MemoryStream(response))
            {
                ResponseData = new byte[response.Length - 2];
                reader.Read(ResponseData, 0, ResponseData.Length);
                
                SW1 = (byte)reader.ReadByte();
                SW2 = (byte)reader.ReadByte();
            }
        }
        /// <summary>
        /// Detects if the command has succeeded
        /// </summary>
        /// <returns></returns>
        public virtual bool Succeeded => SW == 0x9000;

        /// <summary>
        /// command processing status
        /// </summary>
        public byte SW1 { get; set; }
        /// <summary>
        /// command processing qualifier
        /// </summary>
        public byte SW2 { get; set; }
        /// <summary>
        /// Wrapper property to read both response status and qualifer
        /// </summary>
        public ushort SW
        {
            get
            {
                return (ushort)(((ushort)SW1 << 8) | (ushort)SW2);
            }
            set
            {
                SW1 = (byte)(value >> 8);
                SW2 = (byte)(value & 0xFF);
            }
        }
        /// <summary>
        /// Response data
        /// </summary>
        public byte[] ResponseData { get; set; }
        /// <summary>
        /// Mapping response status and qualifer to human readable format
        /// </summary>
        public virtual string SWTranslation
        {
            get
            {
                switch (SW)
                {
                    case 0x9000:
                        return "Success";

                    case 0x6700:
                        return "Incorrect length or address range error";

                    case 0x6800:
                        return "The requested function is not supported by the card";

                    default:
                        return "Unknown";
                }
            }
        }
        /// <summary>
        /// Helper method to print the response in a readable format
        /// </summary>
        /// <returns>
        /// return string formatted response
        /// </returns>
        public override string ToString()
        {
            return "ApduResponse SW=" + SW.ToString("X4") + " (" + SWTranslation + ")" + ((ResponseData != null && ResponseData.Length > 0) ? (",Data=" + BitConverter.ToString(ResponseData).Replace("-", "")) : "");
        }
    }

    internal class NoResponse : ApduResponse
    {
        public override bool Succeeded => false;
    }
    /// <summary>
    /// Class that implements select command
    /// </summary>
    public class SelectCommand : ApduCommand
    {
        public SelectCommand(byte[] aid, byte? le)
            : base((byte)Cla.CompliantCmd0x, (byte)Iso7816.Ins.SelectFile, 0x04, 0x00, aid, le)
        {
        }

        public byte[] AID
        {
            set { CommandData = value; }
            get { return CommandData; }
        }
        public override string ToString()
        {
            return "SelectCommand AID=" + BitConverter.ToString(CommandData).Replace("-", "");
        }
    }
}
