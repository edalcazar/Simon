using System;
using System.IO;
using System.Runtime.Serialization;

namespace Edmon
{
    /// <summary>
    /// Helper class to get and set the record for most completed levels
    /// </summary>
    static class Records
    {
        static string recordHolder;
        static int record;
        static string fileLocation = Environment.CurrentDirectory + "\\RecordInfo.bin";

        public static void SetNewRecord(string name, int level)
        {
            try
            {
                RecordObject recordInfo = new RecordObject();
                recordInfo.recordHolder = name;
                recordInfo.recordLevel = level;
                IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                Stream fileStream = new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(fileStream, recordInfo);
                fileStream.Close();
            }
            catch (Exception)
            {
                // Do nothing if the file is corrupted
            }
            finally
            {
                recordHolder = name;
                record = level;
            }
        }

        public static string RecordHolder()
        {
            if (recordHolder == null)
            {
                RecordObject recordInfo = GetRecordInfo();
                recordHolder = recordInfo.recordHolder;
            }
            return recordHolder;
        }

        public static int RecordLevel()
        {
            if (record == 0)
            {
                RecordObject recordInfo = GetRecordInfo();
                record = recordInfo.recordLevel;
            }
            return record;
        }

        private static RecordObject GetRecordInfo()
        {
            if (File.Exists(fileLocation))
            {
                try
                {
                    IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    Stream serialStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read);
                    RecordObject recordInfo = (RecordObject)formatter.Deserialize(serialStream);
                    serialStream.Close();
                    return recordInfo;
                }
                catch (Exception)
                {
                    return new RecordObject();
                }
            }
            else
                return new RecordObject();
        }

        [Serializable]
        private struct RecordObject
        {
            public string recordHolder;
            public int recordLevel;
        }

    }
}
