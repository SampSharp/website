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

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.Services;

namespace SampSharp.Documentation.Controllers
{
	public class WebHookController : Controller
	{
		private readonly IDocsImportService _docsImportService;
		private readonly IOptions<RepositoryOptions> _repositoryOptions;

		public WebHookController(IDocsImportService docsImportService, IOptions<RepositoryOptions> repositoryOptions)
		{
			_docsImportService = docsImportService;
			_repositoryOptions = repositoryOptions;
		}

		[HttpPost]
		public IActionResult ImportAllBranches()
		{
			if (string.IsNullOrWhiteSpace(_repositoryOptions.Value.Secret))
				Unauthorized();

			if (Request.Headers["Authorization"].FirstOrDefault() == "Bearer " + _repositoryOptions.Value.Secret)
			{
				_docsImportService.ImportAllBranches();
				return NoContent();
			}

			return Unauthorized();
		}

		private bool ValidateRequest(string signature, string body)
		{
			var split = signature.Split('=', 2);

			if (split[0] == "sha1")
			{
				var encoding = Encoding.ASCII;

				var secret = _repositoryOptions.Value.Secret;
				var secretBytes = encoding.GetBytes(secret);
				HMAC hmac = new HMACSHA1(secretBytes);

				var hash = hmac.ComputeHash(encoding.GetBytes(body));

				var checksum = Enumerable.Range(0, split[1].Length)
					.Where(x => x % 2 == 0)
					.Select(x => Convert.ToByte(split[1].Substring(x, 2), 16))
					.ToArray();

				return hash.SequenceEqual(checksum);
			}

			throw new NotImplementedException("Unimplemented signature hashing algorithm");
		}

		public async Task<IActionResult> GitHub()
		{
			if (string.IsNullOrWhiteSpace(_repositoryOptions.Value.Secret))
				Unauthorized();

			var ghEvent = Request.Headers["X-GitHub-Event"].FirstOrDefault();
			var ghDelivery = Request.Headers["X-GitHub-Delivery"].FirstOrDefault();
			var ghHubSignature = Request.Headers["X-Hub-Signature"].FirstOrDefault();

			if (ghEvent == null || ghDelivery == null || ghHubSignature == null) return Unauthorized();

			// https://developer.github.com/webhooks/
			if (ghEvent == "create" || ghEvent == "push" || ghEvent == "delete")
			{
				using (var sr = new StreamReader(Request.Body))
				{
					var body = await sr.ReadToEndAsync();

					if (!ValidateRequest(ghHubSignature, body))
						return Unauthorized();

					await _docsImportService.ImportAllBranches();
				}
			}

			return NoContent();
		}
	}
}