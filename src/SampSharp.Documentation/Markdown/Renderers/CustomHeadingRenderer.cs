// SampSharp.Documentation
// Copyright 2019 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace SampSharp.Documentation.Markdown.Renderers
{
	public class CustomHeadingRenderer : HtmlObjectRenderer<HeadingBlock>
	{
		private static readonly string[] HeadingTexts =
		{
			"h1",
			"h2",
			"h3",
			"h4",
			"h5",
			"h6"
		};

		protected override void Write(HtmlRenderer renderer, HeadingBlock obj)
		{
			var headingText = obj.Level > 0 && obj.Level <= 6
				? HeadingTexts[obj.Level - 1]
				: "<h" + obj.Level.ToString(CultureInfo.InvariantCulture);

			var inline = obj.Inline?.FirstChild as LiteralInline;

			var headingName = EscapeName(inline?.Content.ToString());

			if (renderer.EnableHtmlForBlock)
			{
				if (headingName != null)
				{
					renderer
						.Write("<a name=\"")
						.Write(headingName)
						.WriteLine("\"></a>")
						.Write("<a href=\"#")
						.Write(headingName)
						.WriteLine("\" class=\"title\">");
				}

				renderer.Write("<")
					.Write(headingText)
					.WriteAttributes(obj)
					.Write(">");
			}

			renderer.WriteLeafInline(obj);

			if (renderer.EnableHtmlForBlock)
			{
				renderer.Write("</").Write(headingText).WriteLine(">");

				if (headingName != null) renderer.WriteLine("</a>");
			}
		}

		/// <summary>
		///     Convert a name into a string that can be appended to a Uri.
		/// </summary>
		public static string EscapeName(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				name = NormalizeString(name);

				// Replaces all non-alphanumeric character by a space
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < name.Length; i++)
				{
					builder.Append(char.IsLetterOrDigit(name[i]) ? name[i] : ' ');
				}

				name = builder.ToString();

				// Replace multiple spaces into a single dash
				name = Regex.Replace(name, @"[ ]{1,}", @"-", RegexOptions.None);
			}

			return name;
		}

		/// <summary>
		///     Strips the value from any non english character by replacing thoses with their english equivalent.
		/// </summary>
		/// <param name="value">The string to normalize.</param>
		/// <returns>A string where all characters are part of the basic english ANSI encoding.</returns>
		/// <seealso cref="http://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net" />
		private static string NormalizeString(string value)
		{
			if (value == null)
				return null;

			value = value.ToLower();

			string normalizedFormD = value.Normalize(NormalizationForm.FormD);
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < normalizedFormD.Length; i++)
			{
				UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(normalizedFormD[i]);
				if (uc != UnicodeCategory.NonSpacingMark) builder.Append(normalizedFormD[i]);
			}

			return builder.ToString().Normalize(NormalizationForm.FormC);
		}
	}
}