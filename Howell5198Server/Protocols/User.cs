using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Howell.IO.Serialization;


namespace Howell5198.Protocols
{
    /*
     typedef struct  {
  char name[50];
  char pass[50];
  char email[100];

  // for encrypt 
  int encPass[50];
  int passLen;
  int passTm;
  int privilege;
  unsigned int flags;
  unsigned int loggedIn;

  unsigned int access_cameras;
  unsigned int access_sound;
  unsigned int access_flags;

}user_t;

struct tTempestUsers{
	int userCount;
	user_t *user_item;
};
struct Davinci_user_t {
	char	username[32];
	char	password[16];
	int		privilege;
	char	reserve[32];
};
     */
    public class User:IBytesSerialize
    {
        public User()
        {
            this.Name = null;
            this.Password = null;
            this.Email = null;
            this.EncPass = new Int32[50];
            this.PassLen = 0;
            this.PassTm = 0;
            this.Privilege = 0;
            this.Flags = 0;
            this.LoggedIn = 0;
            this.AccessCameras = 0;
            this.AccessSound = 0;
            this.AccessFlags = 0;
        }

        public String Name { get; set; }
        public String Password { get; set; }
        public String Email { get; set; }
        public Int32[] EncPass { get; set; }
        public Int32 PassLen { get; set; }
        public Int32 PassTm { get; set; }
        public Int32 Privilege { get; set; }
        public UInt32 Flags { get; set; }
        public UInt32 LoggedIn { get; set; }
        public UInt32 AccessCameras { get; set; }
        public UInt32 AccessSound { get; set; }
        public UInt32 AccessFlags { get; set; }
        public int GetLength()
        {
            return 108*4;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Name = LittleEndian.ReadString(50, buffer, ref newOffset, length);
            this.Password = LittleEndian.ReadString(50, buffer, ref newOffset, length);
            this.Email = LittleEndian.ReadString(100, buffer, ref newOffset, length);
            for (int i = 0; i < this.EncPass.Length; ++i)
            {
                this.EncPass[i] = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            }
            this.PassLen = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.PassTm = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Privilege = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            this.Flags = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.LoggedIn = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.AccessCameras = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.AccessSound = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
            this.AccessFlags = LittleEndian.ReadUInt32(buffer, ref newOffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Byte[] Fill = new byte[100];
            Int32 offset = 0;
            Byte[] name = System.Text.Encoding.ASCII.GetBytes(this.Name);
            if (name.Length < 50)
            {
                LittleEndian.WriteBytes(name, 0, name.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 50 - name.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(name, 0, 50, buffer, ref offset, buffer.Length);
            }
            Byte[] password = System.Text.Encoding.ASCII.GetBytes(this.Password);
            if (password.Length < 50)
            {
                LittleEndian.WriteBytes(password, 0, password.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 50 - password.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(password, 0, 50, buffer, ref offset, buffer.Length);
            }
            Byte[] email = System.Text.Encoding.ASCII.GetBytes(this.Email);
            if (email.Length < 100)
            {
                LittleEndian.WriteBytes(email, 0, email.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 100 - email.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(email, 0, 100, buffer, ref offset, buffer.Length);
            }
            for (int i = 0; i < 100; ++i)
            {
                if (i > this.EncPass.Length)
                {
                    LittleEndian.WriteInt32(0, buffer, ref offset, buffer.Length);
                    continue;
                }
                LittleEndian.WriteInt32(this.EncPass[i], buffer, ref offset, buffer.Length);
            }
            LittleEndian.WriteInt32(this.PassLen, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.PassTm, buffer, ref offset, buffer.Length);
            LittleEndian.WriteInt32(this.Privilege, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.Flags, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.LoggedIn, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.AccessCameras, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.AccessSound, buffer, ref offset, buffer.Length);
            LittleEndian.WriteUInt32(this.AccessFlags, buffer, ref offset, buffer.Length);
            return buffer;
        }
    }

    public class TempestUsers : IBytesSerialize
    {
        public Int32 Count { get; set; }
        public User[] Users { get; set; }
        public int GetLength()
        {
            return Users.Length * 432;
        }
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Count = length / 432;
            this.Users = new User[this.Count];
            for(int i=0;i<this.Count;i++)
            {
                this.Users[i] = new User();
                this.Users[i].FromBytes(buffer, i * 432, (length - i * 432));
            }
        }
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            for (int i = 0; i < this.Users.Length; i++)
            {
                Buffer.BlockCopy(this.Users[i].GetBytes(), 0, buffer, i * 432, 432);
            }
            return buffer;
        }
    }

    public class UpdateUserRequest : DavinciUsers { }

    public class UpdateUserResponse : StreamResponse { }


    public class DavinciUser : IBytesSerialize
    {
        public DavinciUser()
        {
            this.Name = null;
            this.Password = null;
            this.Privilege = 0;
            this.Reserve = new Byte[32];
        }

        public String Name { get; set; }
        public String Password { get; set; }
        public Int32 Privilege { get; set; }
        public Byte[] Reserve;

        public int GetLength()
        {
            return 32 + 16 + 4 + 32;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Name = LittleEndian.ReadString(32, buffer, ref newOffset, length);
            this.Password = LittleEndian.ReadString(16, buffer, ref newOffset, length);
            this.Privilege = LittleEndian.ReadInt32(buffer, ref newOffset, length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                this.Reserve[i] = LittleEndian.ReadByte(buffer, ref newOffset, length);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            Byte[] Fill = new byte[32];
            Int32 offset = 0;
            Byte[] name = System.Text.Encoding.ASCII.GetBytes(this.Name);
            if (name.Length < 32)
            {
                LittleEndian.WriteBytes(name, 0, name.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 32 - name.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(name, 0, 32, buffer, ref offset, buffer.Length);
            }
            Byte[] password = System.Text.Encoding.ASCII.GetBytes(this.Password);
            if (password.Length < 16)
            {
                LittleEndian.WriteBytes(password, 0, password.Length, buffer, ref offset, buffer.Length);
                LittleEndian.WriteBytes(Fill, 0, 16 - password.Length, buffer, ref offset, buffer.Length);
            }
            else
            {
                LittleEndian.WriteBytes(password, 0, 16, buffer, ref offset, buffer.Length);
            }
            LittleEndian.WriteInt32(this.Privilege, buffer, ref offset, buffer.Length);
            for (int i = 0; i < this.Reserve.Length; ++i)
            {
                LittleEndian.WriteByte(this.Reserve[i], buffer, ref offset, buffer.Length);
            }
            return buffer;
        }
    }
    public class DavinciUsers : IBytesSerialize
    {
        public Int32 Count { get; set; }
        public DavinciUser[] Users { get; set; }
        public int GetLength()
        {
            return Users.Length * 84;
        }
        public void FromBytes(byte[] buffer, int offset, int length)
        {
            Int32 newOffset = offset;
            this.Count = length / 84;
            this.Users = new DavinciUser[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                this.Users[i] = new DavinciUser();
                this.Users[i].FromBytes(buffer, i * 84, (length - i * 84));
            }
        }
        public byte[] GetBytes()
        {
            Byte[] buffer = new byte[this.GetLength()];
            for (int i = 0; i < this.Users.Length; i++)
            {
                Buffer.BlockCopy(this.Users[i].GetBytes(), 0, buffer, i * 84, 84);
            }
            return buffer;
        }
    }
}
