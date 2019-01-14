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

namespace SampSharp.Documentation.Models
{
	public class DocVersion
	{
		public DocVersion(string tag, string displayName)
		{
			Tag = tag;
			DisplayName = displayName;
		}

		public string Tag { get; set; }

		public string DisplayName { get; set; }

		public string DefaultPage { get; set; }

		#region Equality members

		protected bool Equals(DocVersion other)
		{
			return string.Equals(Tag, other.Tag);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((DocVersion) obj);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Tag != null ? Tag.GetHashCode() : 0;
		}

		/// <summary>
		///     Returns a value that indicates whether the values of two
		///     <see cref="T:SampSharp.Documentation.Repositories.DocVersion" /> objects are equal.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>
		///     true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise,
		///     false.
		/// </returns>
		public static bool operator ==(DocVersion left, DocVersion right)
		{
			return Equals(left, right);
		}

		/// <summary>
		///     Returns a value that indicates whether two <see cref="T:SampSharp.Documentation.Repositories.DocVersion" />
		///     objects have different values.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
		public static bool operator !=(DocVersion left, DocVersion right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}