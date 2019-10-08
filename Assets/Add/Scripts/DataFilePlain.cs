using System;
using System.Text;
using System.IO;
using UnityEngine;
using Ionic.Zlib;

namespace Example
{
    public class DataFilePlain : IDataFile
    {
        // constant
        readonly string PathFormat = Application.persistentDataPath + "/{0}.json.gz";
        readonly int BufferSize = 1024;

        // �ۑ�
        public void Save<T>(T instance, string dataName)
        {
            var filepath = String.Format(PathFormat, dataName);

            // instance �� JSON
            var json = JsonUtility.ToJson(instance);

            using (var outputStream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None)) // ���k�f�[�^ �� �o�C�i���t�@�C��
            using (var compressStream = new GZipStream(outputStream, CompressionMode.Compress)) // MemoryStream �� ���k�f�[�^
            using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))  // Json �� MemoryStream
            {
                int byteCount;
                byte[] buffer = new byte[BufferSize];
                while ((byteCount = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    compressStream.Write(buffer, 0, byteCount);
            }
        }


        // �ǂݍ���
        public T Load<T>(string dataName)
        {
            var filepath = String.Format(PathFormat, dataName);
            if (!File.Exists(filepath))
                return default(T);

            using (var inputStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None)) // �o�C�i���t�@�C�� �� ���k�f�[�^
            using (var compressStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                // ���k�f�[�^ �� MemoryStream
                int byteCount;
                byte[] buffer = new byte[BufferSize];
                while ((byteCount = compressStream.Read(buffer, 0, buffer.Length)) > 0)
                    outputStream.Write(buffer, 0, byteCount);

                // MemoryStream �� Json
                var json = Encoding.UTF8.GetString(outputStream.ToArray());

                // JSON �� instance
                T instance = JsonUtility.FromJson<T>(json);

                return instance;
            }
        }
    }
}