using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace PGReliefMoreForGit.Models.Settings
{
    /// <summary>
    /// ユーザーが設定ファイルとして個別に保存する設定（Singleton）
    /// </summary>
    public sealed class FileSetting
    {
        /// <summary>
        /// Git リポジトリへのパス
        /// </summary>
        public string Repository { get; set; } = string.Empty;
        /// <summary>
        /// 比較対象のコミットの SHA ハッシュ値
        /// </summary>
        public string ShaHash { get; set; } = string.Empty;
        /// <summary>
        /// 入力ファイル（PGRelief で出力した html ファイル）
        /// </summary>
        public string InputFile { get; set; } = string.Empty;
        /// <summary>
        /// 出力ファイル
        /// </summary>
        public string OutputFile { get; set; } = string.Empty;

        /// <summary>
        /// 自分のインスタンス
        /// </summary>
        public static FileSetting Instance { get; set; } = new FileSetting();

        /// <summary>
        /// コンストラクタ（Singleton）
        /// </summary>
        private FileSetting() { }

        public void Load(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                Instance = JsonConvert.DeserializeObject<FileSetting>(sr.ReadToEnd());
            }
        }

        public void Save(string fileName)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(json);
            }
        }
    }
}
