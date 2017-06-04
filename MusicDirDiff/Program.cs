using MusicDirDiff.Properties;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MusicDirDiff {
    class Program {
        public const string log = "Musig_Log.txt";
        static string Mymusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        static string BackupDir = ConfigurationManager.AppSettings["backupdir"];
        public static bool logexsist = File.Exists(log);

        static void Main() {
            //ログファイルを確認
            CheckLog();

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

        private static void CheckLog() {
            if (!logexsist) {
                Console.WriteLine("ログファイルが見つからないので、リセットします");
                Settings.Default.Reset();
                Console.WriteLine("リセットしました\n");
            }
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
            Console.WriteLine("ログファイルを更新中...");

            //Append to text         
            string content = $"{DateTime.Now} - {FileSizeHelpler.SizeSuffix(NewSize)} ({NewSize:N0} バイト)";
            content = Program.logexsist ? $"\n{content}" : content;
            File.AppendAllText(Program.log, content);

            //save to setting
            Settings.Default.LastSyncSize = NewSize;
            Settings.Default.Save();

            Console.WriteLine("ログファイルを更新しました...");
            Process.Start(Program.log);
        }
    }
}