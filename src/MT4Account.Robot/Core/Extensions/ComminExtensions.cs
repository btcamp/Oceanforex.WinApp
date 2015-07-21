using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MT4Account.Robot
{
    public static class ComminExtensions
    {
        public static List<T> Clone<T>(this IEnumerable<T> list)
        {
            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, list);
                stream.Seek(0, SeekOrigin.Begin);
                return (List<T>)formatter.Deserialize(stream);
            }
        }
    }
}
