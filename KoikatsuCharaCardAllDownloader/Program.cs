using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net;
using HtmlAgilityPack;

namespace KoikatsuCharaCardAllDownloader
{
	/// <summary>
	/// コイカツ公式キャラクターアップローダーからキャラクターカードをダウンロードするやつ。
	/// </summary>
	class Program
	{


		/// <summary>
		/// None
		/// </summary>
		/// <param name="args">0:男、1:女</param>
		static void Main(string[] args)
		{
			var directory = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "OUTPUT");
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			if (!Directory.Exists(Path.Combine(directory, "female")))
			{
				Directory.CreateDirectory(Path.Combine(directory, "female"));
			}
			if (!Directory.Exists(Path.Combine(directory, "male")))
			{
				Directory.CreateDirectory(Path.Combine(directory, "male"));
			}
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			var charaDic = new List<Tuple<string, string, string>>();

			using (var client = new WebClient())
			{
				client.Encoding = Encoding.GetEncoding("euc-jp");
				client.Headers.Add("Host", "up.illusion.jp");
				client.Headers.Add("Referer", "http://up.illusion.jp/");
				var url = "http://up.illusion.jp/koikatu_upload/chara/ajax/search-ajax.php?" + UploaderPramQuery.CreateUrlQuery(1, args[0].Equals("1"));
				var result = client.DownloadString(url).Replace("\r", "").Replace("\n", "").Replace("\t", "");
				var doc = new HtmlDocument();
				doc.LoadHtml(result);
				var maxPage = int.Parse(doc.DocumentNode.SelectNodes(@"//div[@class=""pagepn""] //a")[10].InnerText);
				Console.WriteLine(args[0].Equals("1") ? "女キャラ取得" : "男キャラ取得");
				Console.WriteLine("一覧取得中。合計:" + maxPage + "回");
				for (int i = 1; maxPage >= i; i++)
				{

					var url2 = "http://up.illusion.jp/koikatu_upload/chara/ajax/search-ajax.php?" + UploaderPramQuery.CreateUrlQuery(i, args[0].Equals("1"));
					Console.WriteLine(i + " URL: " + url2);
					var result1 = client.DownloadString(url2).Replace("\r", "").Replace("\n", "").Replace("\t", "");
					var doclist = new HtmlDocument();
					doclist.LoadHtml(result1);

					foreach (var item in doclist.DocumentNode.SelectNodes(@"//ul[@class=""uc_img""]"))
					{
						//sample
						/*
							<li><a href="download.php/security/koikatu0084611/products_id/84611/cPath/0"><img src="updata/koikatu0084611.png"></a></li>
							<li><a href="index.php?hn=9077">fkv2</a></li>
							<li>2020-07-25</li>
							<li><img src="img/chara/s_w.png"></li>
							<li><img src="img/chara/p_w_20.png"></li>
							<li><img src="img/chara/b_0.png"></li>
							<li><img src="img/chara/t_short.png"></li>
							<li>コメントがありません</li>
							<li><a href="download.php/security/koikatu0084611/products_id/84611/cPath/0"><img src="img/chara/dl.png"></a></li>
						 */
						var child = item.ChildNodes;
						var imgUrl = "http://up.illusion.jp/koikatu_upload/chara/" + child[0].ChildNodes[0].GetAttributes("href").First().Value;
						var creator = child[1].ChildNodes[0].InnerText;
						var imgId = child[0].ChildNodes[0].GetAttributes("href").First().Value.Split("/")[4];
						charaDic.Add(new Tuple<string, string, string>(imgUrl, imgId, creator));
					}
				}
				Console.WriteLine("一覧取得完了");
				Console.WriteLine("合計：" + charaDic.Count);

				var head = new WebHeaderCollection();
				head.Add("Host", "up.illusion.jp");
				head.Add("Referer", "http://up.illusion.jp/");
				Console.WriteLine("ファイル取得中");
				foreach (var item in charaDic)
				{
					var file = item.Item2 + "_" + item.Item3 + ".png";
					file = file.Replace("\"", "”").Replace("<", "＜").Replace(">", "＞").Replace("|", "｜").Replace(":", "：").Replace("*", "＊").Replace("\\", "￥").Replace("/", "／").Replace("?", "？");
					var fileName = args[0].Equals("1") ? Path.Combine(directory, "female", file) : Path.Combine(directory, "male", file);
					if (!File.Exists(fileName))
					{
						Console.WriteLine((charaDic.IndexOf(item) + 1) + " 新規：" + item.Item1);
						client.Headers = new WebHeaderCollection();
						client.Headers.Add("Host", "up.illusion.jp");
						client.Headers.Add("Referer", "http://up.illusion.jp/");
						client.DownloadFile(item.Item1, fileName);
					}
					else
					{
						Console.WriteLine((charaDic.IndexOf(item) + 1) + " 既存：" + item.Item1);
					}
				}
				Console.WriteLine("ファイル取得完了");
			}



		}
	}
}
