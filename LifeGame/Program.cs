using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;
using SdlDotNet.Input;
using Font = SdlDotNet.Graphics.Font;

namespace LifeGame
{
	static class Program
	{
		// ライフゲームの大きさ
		// データファイルもこのサイズにしないとダメ
		const int Width = 100;
		const int Height = 100;

		// 1生き物の大きさ
		const int Dot = 5;

		// スリープ
		const int Sleep = 0;

		// ロジックにC#を使うか、F#を使うか
		const bool csharp = false;

        /// <summary>
        /// ファイルからデータ作成
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>作成したデータ</returns>
        static private List<bool> Initialize(string fileName)
        {
            var data = new List<bool>();
            // データファイル読み込み
            // Width桁×Height行のテキストファイル
            // ' '(空白)は何もない、'1'(空白以外)は生き物がいる
            using (var sr = new StreamReader(fileName))
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    foreach (var c in line)
                        data.Add(c != ' ');
                    line = sr.ReadLine();
                }
            }
            return data;
        }

		/// <summary>
		/// まいんちゃん
		/// </summary>
		/// <param name="args">データファイル名</param>
		[STAThread]
		static private void Main(string[] args)
		{
			// 引数でデータファイル名を指定する
			if (args.Length != 1)
			{
				Console.WriteLine("LifeGame DATAFILE");
				return;
			}
			var initData = Initialize(args[0]);
            var data = initData;
			var start = false;
			// 画面作成
			Video.WindowCaption = "HELLO SDL.NET WORLD";
			Surface screen = Video.SetVideoMode(Width * Dot, Height * Dot);
			// イベントハンドラ設定
			Events.Quit += (object s, QuitEventArgs e) => { Events.QuitApplication(); };
			Events.MouseButtonDown += (object s, MouseButtonEventArgs e) => { start = !start; };
			Events.Tick += (object s, TickEventArgs e) =>
			{
                // ライフゲーム本体
				screen.Fill(Color.Black);
				using (var font = new Font(@"GenShinGothic-Normal.ttf", 24))
				{
					if (!start)
					{
						using (var surface = font.Render("Click to start !!", Color.Yellow))
						{
							screen.Blit(surface, new Point(Width / 2, Height / 2));
						}
					}
					using (var surface = font.Render(csharp ? "C#モード" : "F#モード", Color.Red))
					{
						screen.Blit(surface, new Point(0, 0));
					}
				}
				Draw(screen, data);
				Video.Update();
				if (start)
					if (csharp)
						data = Logic(data); // C#
					else
	                    data = LifeGameLogic.logic(data, Width, Height).ToList(); // F#
				else
					data = initData;
				if (Sleep > 0)
					Thread.Sleep(Sleep);
			};
			Events.Run();
		}
        /// <summary>
        /// 周辺の生き物の数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        static private int GetLifePoint(int x, int y, List<bool> data)
        {
            var count = 0;
            for (var y1 = y - 1; y1 <= y + 1; ++y1)
            {
                for (var x1 = x - 1; x1 <= x + 1; ++x1)
                {
                    if (x1 != x || y1 != y)
                    {
                        if (data[((y1 < 0 ? y1 + Height : y1) % Height) * Width + ((x1 < 0 ? x1 + Width : x1) % Width)])
                            ++count;
                    }
                }
            }
            return count;
        }
		/// <summary>
		/// 指定された座標の生き物が生きるかどうか？
		/// </summary>
		/// <param name="data"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		static private bool isSurvive(List<bool> data, int x, int y)
		{
			var count = GetLifePoint(x, y, data);
			var dot = data[y * Width + x];
			if (dot)
			{
				// 生存
				// 過疎・過密
				if (count != 2 && count != 3)
					return false;
			}
			else
			{
				// 誕生
				if (count == 3)
					return true;
			}
			return dot;
		}
        /// <summary>
        /// ライフゲームメインロジック
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static private List<bool> Logic(List<bool> data)
        {
            var res = new List<bool>();
            for (var y = 0; y < Height; ++y)
                for (var x = 0; x < Width; ++x)
                    res.Add(isSurvive(data, x, y));
            System.Diagnostics.Debug.Assert(data.Count == res.Count);
            return res;
        }

		/// <summary>
		/// 描画
		/// </summary>
		/// <param name="screen">SDLサーフェイス</param>
		/// <param name="data">描画データ(Width×Height)</param>
		static private void Draw(Surface screen, List<bool> data)
		{
			for (var y = 0; y < Height; ++y)
			{
				for (var x = 0; x < Width; ++x)
				{
					var rect = new Rectangle(x * Dot, y * Dot, Dot, Dot);
					if (data[y * Width + x])
						screen.Fill(rect, Color.White);
				}
			}
		}
	}
}
