using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Edmon
{
    /// <summary>
    /// Helper class to get and set the record for most completed levels
    /// </summary>
    static class Records
    {
        // User must have write access to CurrentDirectory for this functionality to work correctly
        static readonly string fileLocation = Environment.CurrentDirectory + "\\RecordInfo.bin";
        static RecordObject recordInfo;

        public static string RecordHolder
        {
            get
            {
                if (string.IsNullOrEmpty(recordInfo.recordHolder))
                    recordInfo = GetRecordInfo();
                return recordInfo.recordHolder;
            }
        }

        public static int RecordLevel
        {
            get
            {
                if (recordInfo.recordLevel == 0)
                    recordInfo = GetRecordInfo();
                return recordInfo.recordLevel;
            }
        }

        /// <summary>
        /// Get the record holder information from local file
        /// </summary>
        /// <returns></returns>
        private static RecordObject GetRecordInfo()
        {
            if (File.Exists(fileLocation))
            {
                try
                {
                    IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    using (Stream serialStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        RecordObject recordInfo = (RecordObject)formatter.Deserialize(serialStream);
                        return recordInfo;
                    }
                }
                catch (Exception)
                {
                    // Do nothing if the file can't be read (e.g., corrupted due to manual editing)
                    // Just return blank RecordObject
                    return new RecordObject();
                }
            }
            else
                return new RecordObject();
        }

        /// <summary>
        /// Serialize the new record info to file
        /// </summary>
        /// <param name="name">Name of record holder</param>
        /// <param name="level">Number of game levels reached</param>
        public static void SetNewRecord(string name, int level)
        {
            recordInfo.recordHolder = name;
            recordInfo.recordLevel = level;
            try
            {
                IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (Stream fileStream = new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(fileStream, recordInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error saving your record to disk:\n" + ex.Message);
            }
        }

        [Serializable]
        private struct RecordObject
        {
            public string recordHolder;
            public int recordLevel;
        }

    }
}
