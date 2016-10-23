using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Dynamic;
using Newtonsoft.Json;

namespace scorpio_lib
{
    public class NetworkMessage
    {
        Dictionary<string, byte[]> _data = new Dictionary<string, byte[]>();
        int _data_iterator = 0;

        static readonly byte[] header_magic_byte = new byte[4] { 0xAF, 0xFA, 0x0F, 0xA0 };

        public NetworkMessage() { }
        public NetworkMessage(byte[] data)
        {
            string json_string = Encoding.Unicode.GetString(data);

            try
            {
                _data = JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(json_string);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.ToString()}");
            }
        }


        public void AttachString(string value, string key = null)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = Encoding.Unicode.GetBytes(value);
        }
        public void AttachInt32(int value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = BitConverter.GetBytes(value);
        }
        public void AttachUInt32(uint value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = BitConverter.GetBytes(value);
        }
        public void AttachFloat32(float value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = BitConverter.GetBytes(value);
        }
        public void AttachDouble(double value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = BitConverter.GetBytes(value);
        }
        public void AttachBool(bool value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = BitConverter.GetBytes(value);
        }
        public void AttachChar(char value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            _data[key] = BitConverter.GetBytes(value);
        }

        public void AttachFloat32Table(float[] value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            byte[] bytes = new byte[sizeof(float) * value.Length];

            for (int i = 0; i < value.Length; i++)
            {
                BitConverter.GetBytes(value[i]).CopyTo(bytes, i * sizeof(float));
            }

            _data[key] = bytes;
        }
        public void AttachInt32Table(int[] value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            byte[] bytes = new byte[sizeof(int) * value.Length];

            for (int i = 0; i < value.Length; i++)
            {
                BitConverter.GetBytes(value[i]).CopyTo(bytes, i * sizeof(int));
            }

            _data[key] = bytes;
        }
        public void AttachUInt32Table(uint[] value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            byte[] bytes = new byte[sizeof(uint) * value.Length];

            for (int i = 0; i < value.Length; i++)
            {
                BitConverter.GetBytes(value[i]).CopyTo(bytes, i * sizeof(uint));
            }

            _data[key] = bytes;
        }
        public void AttachDoubleTable(double[] value, string key)
        {
            if (key == null)
            {
                key = _data_iterator.ToString();
                _data_iterator++;
            }

            byte[] bytes = new byte[sizeof(double) * value.Length];

            for (int i = 0; i < value.Length; i++)
            {
                BitConverter.GetBytes(value[i]).CopyTo(bytes, i * sizeof(double));
            }

            _data[key] = bytes;
        }


        public byte[] GetSerialized()
        {
            string data = JsonConvert.SerializeObject(_data);
            byte[] bin_data = Encoding.Unicode.GetBytes(data);

            // create header
            byte[] message = new byte[8];

            // header magic number
            message[0] = header_magic_byte[0];
            message[1] = header_magic_byte[1];
            message[2] = header_magic_byte[2];
            message[3] = header_magic_byte[3];

            // length of message
            BitConverter.GetBytes(bin_data.Length).CopyTo(message, 4);

            // add data to message
            message = message.Concat(bin_data).ToArray();

            return message;
        }

        public class HeaderInfo
        {
            public enum ReturnCode { HeaderFound, HeaderFoundPartially, HeaderNotFound}

            public ReturnCode return_code = ReturnCode.HeaderFound;
            public int data_start_index = -1;
            public int data_size = -1;
            public int bytes_saved = -1;

        }

        /*
         * Returns elemeny saved under given key, or throw exeption if not found. For strings null is returned.
         */
        public string GetElementString(string key)
        {
            byte[] value;

            if ( _data != null && _data.TryGetValue(key, out value))
            {
                return Encoding.Unicode.GetString(value);
            }
            else
            {
                return null;
            }
        }
        public int GetElementInt32(string key)
        {
            byte[] value;

            if ( _data != null && _data.TryGetValue(key, out value))
            {
                return BitConverter.ToInt32(value, 0);
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }
        public uint GetElementUInt32(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                return BitConverter.ToUInt32(value, 0);
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }
        public float GetElementFloat32(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                return BitConverter.ToSingle(value, 0);
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }
        public double GetElementDouble(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                return BitConverter.ToDouble(value, 0);
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }
        public bool GetElementBool(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                return BitConverter.ToBoolean(value, 0);
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }
        public char GetElementChar(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                return BitConverter.ToChar(value, 0);
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }

        public float[] GetElementFloat32Table(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                if (value.Length % sizeof(float) != 0)
                {
                    throw new Exception($"Variable with key '{key} has wrong size!'");
                }

                float[] table = new float[value.Length / sizeof(float)];

                for (int i = 0; i < table.Length; i++)
                {
                    BitConverter.ToSingle(value, i * sizeof(float));
                }

                return table;
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }
        public int[] GetElementInt32Table(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                if (value.Length % sizeof(int) != 0)
                {
                    throw new Exception($"Variable with key '{key} has wrong size!'");
                }

                int[] table = new int[value.Length / sizeof(int)];

                for (int i = 0; i < table.Length; i++)
                {
                    BitConverter.ToSingle(value, i * sizeof(int));
                }

                return table;
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }
        public uint[] GetElementUInt32Table(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                if (value.Length % sizeof(uint) != 0)
                {
                    throw new Exception($"Variable with key '{key} has wrong size!'");
                }

                uint[] table = new uint[value.Length / sizeof(uint)];

                for (int i = 0; i < table.Length; i++)
                {
                    BitConverter.ToSingle(value, i * sizeof(uint));
                }

                return table;
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }
        public double[] GetElementDoubleTable(string key)
        {
            byte[] value;

            if (_data != null && _data.TryGetValue(key, out value))
            {
                if (value.Length % sizeof(double) != 0)
                {
                    throw new Exception($"Variable with key '{key} has wrong size!'");
                }

                double[] table = new double[value.Length / sizeof(double)];

                for (int i = 0; i < table.Length; i++)
                {
                    BitConverter.ToSingle(value, i * sizeof(double));
                }

                return table;
            }
            else
            {
                throw new Exception($"Variable with key '{key} not found!'");
            }
        }


        public static HeaderInfo SearchForMessage(byte[] data)
        {
            HeaderInfo header_info = new HeaderInfo();

            // scan data stream for magic numbers
            int magic_byte_number = 0;
            int i = 0;
            for(; i < data.Length; i++)
            {
                if (data[i] == header_magic_byte[magic_byte_number])
                    magic_byte_number++;
                else
                    magic_byte_number = 0;

                if (magic_byte_number == 4)
                    break;
            }
            i++;

            // return message size if header magic number was found 
            if (magic_byte_number == 4 && i + 4 < data.Length)
            {
                header_info.data_size = BitConverter.ToInt32(data, i);
                header_info.return_code = HeaderInfo.ReturnCode.HeaderFound;
                header_info.data_start_index = i + 4;
            }
            else if (magic_byte_number > 0)
            {
                header_info.return_code = HeaderInfo.ReturnCode.HeaderFoundPartially;
                header_info.bytes_saved = magic_byte_number + data.Length - i + 1;
            }
            else
            {
                header_info.return_code = HeaderInfo.ReturnCode.HeaderNotFound;
            }

            return header_info;

        }
    }
}
