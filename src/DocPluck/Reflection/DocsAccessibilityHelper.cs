// DocPluck
// Copyright 2020 Tim Potze
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

namespace DocPluck.Reflection
{
	/// <summary>
	/// Provides helper methods for the <see cref="DocsAccessibilityLevel"/> enumeration.
	/// </summary>
	public static class DocsAccessibilityHelper
	{
		/// <summary>
		/// Converts the specified accessibility level to a string.
		/// </summary>
		/// <param name="accessibilityLevel">The accessibility level.</param>
		/// <returns>
		/// A <see cref="string" /> that represents the accessibility level.
		/// </returns>
		public static string ToString(DocsAccessibilityLevel accessibilityLevel)
		{
			return accessibilityLevel switch
			{
				DocsAccessibilityLevel.Public => "public",
				DocsAccessibilityLevel.Internal => "internal",
				DocsAccessibilityLevel.Private => "private",
				DocsAccessibilityLevel.Protected => "protected",
				DocsAccessibilityLevel.ProtectedInternal => "protected internal",
				DocsAccessibilityLevel.PrivateProtected => "private protected",
				_ => "???"
			};
		}

		private static int GetRestrictivenessValue(DocsAccessibilityLevel accessibilityLevel)
		{
			return accessibilityLevel switch
			{
				DocsAccessibilityLevel.Public => 0,
				DocsAccessibilityLevel.Internal => 2,
				DocsAccessibilityLevel.Private => 4,
				DocsAccessibilityLevel.Protected => 2,
				DocsAccessibilityLevel.ProtectedInternal => 1,
				DocsAccessibilityLevel.PrivateProtected => 3,
				_ => 0
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="value" /> is more restrictive than the specified
		/// <paramref name="other" /> value.
		/// </summary>
		/// <param name="value">The value to compare the other value with.</param>
		/// <param name="other">The other value to compare to the first value..</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="value" /> is more restrictive than the specified <paramref name="other" />
		/// value; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsMoreRestrictive(DocsAccessibilityLevel value, DocsAccessibilityLevel other)
		{
			return GetRestrictivenessValue(value) > GetRestrictivenessValue(other);
		}
	}
}