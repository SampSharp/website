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

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.NewModels;

namespace SampSharp.Documentation.Repositories
{
	public class GithubDataRepository : IGithubDataRepository
	{
		private readonly GitHubClient _client;
		private readonly IOptions<RepositoryOptions> _options;

		public GithubDataRepository(IOptions<RepositoryOptions> options)
		{
			_options = options;
			_client = new GitHubClient(new ProductHeaderValue("SampSharpDocs"));
		}
		
		public async Task<GithubBranch[]> GetBranches()
		{
			var repo = await _client.Repository.Get(_options.Value.Owner, _options.Value.Name);
			var branches = await _client.Repository.Branch.GetAll(_options.Value.Owner, _options.Value.Name);

			var defaultBranch = repo.DefaultBranch;

			return branches.Select(b => new GithubBranch
			{
				Name = b.Name,
				Sha = b.Commit.Sha,
				IsDefault = b.Name == defaultBranch
			}).ToArray();
		}

		public async Task<GithubBranch> GetBranch(string branchName)
		{
			var branch = await _client.Repository.Branch.Get(_options.Value.Owner, _options.Value.Name, branchName);

			if (branch == null)
				return null;

			var repo = await _client.Repository.Get(_options.Value.Owner, _options.Value.Name);

			var defaultBranch = repo.DefaultBranch;

			return new GithubBranch
			{
				Name = branch.Name,
				Sha = branch.Commit.Sha,
				IsDefault = branch.Name == defaultBranch
			};
		}

		public string PublicUrl => $"https://github.com/{_options.Value.Owner}/{_options.Value.Name}";

		public Task<byte[]> GetArchive(GithubBranch branch)
		{
			return _client.Repository.Content.GetArchive(_options.Value.Owner, _options.Value.Name, ArchiveFormat.Tarball, branch.Sha);
		}
	}
}