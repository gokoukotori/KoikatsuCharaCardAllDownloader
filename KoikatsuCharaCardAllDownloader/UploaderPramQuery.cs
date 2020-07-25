using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoikatsuCharaCardAllDownloader
{
	public static class UploaderPramQuery
	{
		/// <summary>
		/// クエリ作成
		/// </summary>
		/// <param name="page">25件ずつ</param>
		/// <param name="sex">true;女 false:男</param>
		public static string CreateUrlQuery(int page, bool sex)
		{
			var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

			queryString["pg"] = page.ToString();
			if (sex)
				queryString["sex1"] = "true";
			else
				queryString["sex0"] = "true";

			queryString["rank1"] = "true";

			return queryString.ToString();
		}


	}
}
