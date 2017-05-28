using MusicDirDiff.Properties;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace MusicDirDiff {
    class Program {
        static void Main() {

            //パスをGET
            string Mymusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string BackupDir = ConfigurationManager.AppSettings["backupdir"];
            //DIRオブジェクトを生成
            Dir dir_org = new Dir("オリジナル", Mymusic);
            Dir dir_backup = new Dir("バックアップ", BackupDir);
            //SizeCheckerオブジェクトを生成

            if (dir_org.Isexsist && dir_backup.Isexsist) {
                SizeChecker sc = new SizeChecker(dir_backup.Size, dir_org.Size, Settings.Default.LastSyncSize);
                if (sc.Confirm()) sc.Update();
                else Console.WriteLine("\n\n更新を実行しません");
            }

            Console.WriteLine("続行するには何かキーを押してください. . .");
            Console.ReadLine();
        }
    }

    public class Dir {
        public bool Isexsist { get; private set; } = false;
        public long Size { get; private set; } = 0;

        public Dir(string name, string path) {

            Isexsist = Directory.Exists(path);
            if (!Isexsist) {
                Console.WriteLine($"{name} パス : {path} が見つかりません");
                return;
            }

            Console.WriteLine($"{name} パス : {path}");

            //パスの容量を計算
            Caculatesize(name, path);
        }

        void Caculatesize(string name, string path) {
            DirectoryInfo di = new DirectoryInfo(path);
            Size = di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            Console.WriteLine($"{name}のサイズ : {Size}\n");
        }
    }

    public class SizeChecker {

        public long LastSyncSize { get; private set; } = 0;
        public long NewSize { get; private set; } = 0;
        public bool NeedtoUpdate { get; private set; } = false;

        public SizeChecker(long backsize, long orgsize, long lastsize) {

            Console.WriteLine($"前回の同期サイズ : {lastsize}");

            if (!orgsize.Equals(backsize)) {
                Console.WriteLine(" ------------------------------------------");
                Console.WriteLine("｜オリジナルとバックアップのサイズが違う！｜");
                Console.WriteLine(" ------------------------------------------");
                return;
            }

            if (orgsize.Equals(lastsize)) {
                Console.WriteLine("おめでとう！何も変わってないよ");
                return;
            }

            LastSyncSize = lastsize;
            NewSize = orgsize;
            NeedtoUpdate = true;
        }

        public bool Confirm() {
            if (!NeedtoUpdate) return false;

            string consolestring = NewSize > LastSyncSize ? "大きくなった" : "小さくなった";
            Console.Write($"{consolestring}けど、更新しますか？(Y/N):");
            var key = Console.ReadKey();
            return key.Key == ConsoleKey.Y;
        }

        public void Update() {
            Console.WriteLine("更新中...");

            //save to setting
            Settings.Default.LastSyncSize = NewSize;
            Settings.Default.Save();
            //Append to text
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"更新時間 : {DateTime.Now}");
            sb.AppendLine($"同期サイズ : {FileSizeHelpler.SizeSuffix(NewSize)} ({NewSize})");
            sb.AppendLine("---------------------------------------");
            File.AppendAllText("Musig_Log.txt", sb.ToString());

            Console.WriteLine("更新しました...");
        }
    }
}