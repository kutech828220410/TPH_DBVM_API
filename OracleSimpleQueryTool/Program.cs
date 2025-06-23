using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace OracleSimpleQueryTool
{
    class Program
    {
        const string ConnFile = "last_conn.txt";
        const string SqlFile = "last_sql.txt";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.WriteLine("=== Oracle 查詢工具 v3（重連機制 + 查詢記憶）===");

            string connStr = LoadLastUsed(ConnFile, "連線字串");
            string lastSql = LoadLastUsed(SqlFile, "查詢語句");

            while (true)
            {
                Console.Write("\n請輸入查詢語句（留空使用上次查詢，exit 離開）：\n> ");
                string? sql = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(sql))
                {
                    sql = lastSql;
                    Console.WriteLine($"[使用上次查詢]：{sql}");
                }

                if (sql.ToLower() == "exit") break;
                if (string.IsNullOrWhiteSpace(sql))
                {
                    Console.WriteLine("[錯誤] 無任何查詢語句！");
                    continue;
                }

                var sw = new Stopwatch();
                Console.WriteLine("[連線中]...");
                sw.Start();

                using (var conn = new OracleConnection(connStr))
                {
                    try
                    {
                        conn.Open();
                        sw.Stop();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[成功] 連線完成，耗時 {sw.ElapsedMilliseconds} ms");
                        Console.ResetColor();
                        SaveToFile(ConnFile, connStr);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[錯誤] 連線失敗，耗時 {sw.ElapsedMilliseconds} ms");
                        Console.WriteLine("錯誤：" + ex.Message);
                        Console.ResetColor();
                        continue;
                    }

                    Console.WriteLine("[查詢中]...");
                    sw.Restart();

                    try
                    {
                        using (var cmd = new OracleCommand(sql, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                                Console.Write(reader.GetName(i).PadRight(15) + "\t");
                            Console.WriteLine("\n" + new string('-', 60));

                            int rowCount = 0;
                            while (reader.Read() && rowCount < 100)
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                    Console.Write(reader[i]?.ToString()?.PadRight(15) + "\t");
                                Console.WriteLine();
                                rowCount++;
                            }

                            sw.Stop();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n[完成] 查詢耗時：{sw.ElapsedMilliseconds} ms，共顯示 {rowCount} 筆（最多 100）");
                            Console.ResetColor();
                            SaveToFile(SqlFile, sql); // 記住查詢
                            lastSql = sql;
                        }
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[查詢錯誤] 耗時：{sw.ElapsedMilliseconds} ms");
                        Console.WriteLine("錯誤：" + ex.Message);
                        Console.ResetColor();
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n（按 Enter 繼續查詢）");
                Console.ResetColor();
                Console.ReadLine();
            }

            Console.WriteLine("\n已結束，按任意鍵離開...");
            Console.ReadKey();
        }

        static string LoadLastUsed(string file, string label)
        {
            if (File.Exists(file))
            {
                string content = File.ReadAllText(file).Trim();
                Console.WriteLine($"偵測到上次使用的{label}：\n{content}");
                Console.Write($"是否使用上次的{label}？(Y/n)：");
                string? ans = Console.ReadLine()?.Trim().ToLower();
                if (ans == "y" || string.IsNullOrWhiteSpace(ans))
                    return content;
            }

            Console.Write($"請輸入新的{label}：");
            return Console.ReadLine()?.Trim();
        }

        static void SaveToFile(string file, string content)
        {
            try
            {
                File.WriteAllText(file, content.Trim());
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[警告] 無法儲存 {file}");
                Console.ResetColor();
            }
        }
    }
}
