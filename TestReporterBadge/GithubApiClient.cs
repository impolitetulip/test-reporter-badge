﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TestReporterBadge
{
    public class GithubApiClient
    {
        public string org;
        public string owner;
        public string repo;
        public string user;
        public string branch;
        public string job;

        RestClient client = new RestClient();

        public GithubApiClient()
        {

        }

        public string GetLatestJobsUrl()
        {
            RestRequest request = new RestRequest("https://api.github.com/repos/" + owner + '/' + repo + "/actions/runs");
            IRestResponse response;

            try
            {
                response = client.Get(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("ERROR - HTTP status code = " + response.StatusCode.ToString());
                }

                JObject jsonResponse = JsonConvert.DeserializeObject<JObject>(response.Content);
                uint totalCount = uint.Parse(jsonResponse.SelectToken("total_count").ToString());
                JToken workflowRunTokens = jsonResponse.SelectToken("workflow_runs");
                JToken workflowRunToken;
                string headBranch;

                if (branch == null)
                {
                    workflowRunToken = workflowRunTokens.SelectToken("[0]");
                    string jobsUrl = workflowRunToken.SelectToken("jobs_url").ToString();
                    return jobsUrl;
                }
                else
                {
                    for (uint i = 0; i < totalCount; i++)
                    {
                        workflowRunToken = workflowRunTokens.SelectToken('[' + i.ToString() + ']');
                        headBranch = workflowRunToken.SelectToken("head_branch").ToString();

                        if (headBranch == branch)
                        {
                            string jobsUrl = workflowRunToken.SelectToken("jobs_url").ToString();
                            return jobsUrl;
                        }

                    }
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        public string GetLatestTestUrl()
        {
            string jobsUrl = GetLatestJobsUrl();
            RestRequest request = new RestRequest(jobsUrl);
            IRestResponse response;

            try
            {
                response = client.Get(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("ERROR - HTTP status code = " + response.StatusCode.ToString());
                }

                JObject jsonResponse = JsonConvert.DeserializeObject<JObject>(response.Content);
                uint totalCount = uint.Parse(jsonResponse.SelectToken("total_count").ToString());
                JToken jobsToken = jsonResponse.SelectToken("jobs");
                JToken jobToken;
                string jobName;

                for (uint i = 0; i < totalCount; i++)
                {
                    jobToken = jobsToken.SelectToken('[' + i.ToString() + ']');
                    jobName = jobToken.SelectToken("name").ToString();

                    if(jobName == job)
                    {
                        string checkRunUrl = jobToken.SelectToken("check_run_url").ToString();
                        return checkRunUrl;
                    }
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        public string GetLatestTestHtmlUrl()
        {
            string jobsUrl = GetLatestJobsUrl();
            RestRequest request = new RestRequest(jobsUrl);
            IRestResponse response;

            try
            {
                response = client.Get(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("ERROR - HTTP status code = " + response.StatusCode.ToString());
                }

                JObject jsonResponse = JsonConvert.DeserializeObject<JObject>(response.Content);
                uint totalCount = uint.Parse(jsonResponse.SelectToken("total_count").ToString());
                JToken jobsToken = jsonResponse.SelectToken("jobs");
                JToken jobToken;
                string jobName;

                for (uint i = 0; i < totalCount; i++)
                {
                    jobToken = jobsToken.SelectToken('[' + i.ToString() + ']');
                    jobName = jobToken.SelectToken("name").ToString();

                    if (jobName == job)
                    {
                        string htmlUrl = jobToken.SelectToken("html_url").ToString();
                        return htmlUrl;
                    }
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        public string GetLatestTestBadgeUrl()
        {
            string checkRunsUrl = GetLatestTestUrl();
            RestRequest request = new RestRequest(checkRunsUrl);
            IRestResponse response;

            try
            {
                response = client.Get(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("ERROR - HTTP status code = " + response.StatusCode.ToString());
                }

                JObject jsonResponse = JsonConvert.DeserializeObject<JObject>(response.Content);
                string checkRunsSummary = jsonResponse.SelectToken("output.summary").ToString();
                Regex regex = new Regex(@"https://img.shields.io/badge/.+(?=\))");
                string badgeUrl = regex.Matches(checkRunsSummary)[0].Value;
                return badgeUrl;
            }
            catch (Exception)
            {

            }

            return null;
        }

        public string GetReadMeContents()
        {
            string readmeString = "https://api.github.com/repos/" + owner + '/' + repo + "/contents/README.md";
            RestRequest request = new RestRequest(readmeString);
            IRestResponse restResponse = client.Get(request);

            if (restResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("ERROR - HTTP status code = " + restResponse.StatusCode.ToString());
                return null;
            }

            try
            {
                JObject jsonReadme = JsonConvert.DeserializeObject<JObject>(restResponse.Content);
                byte[] rawReadme = Convert.FromBase64String(jsonReadme.SelectToken("content").ToString());
                string readme = Encoding.UTF8.GetString(rawReadme);
                return readme;
            }
            catch (Exception)
            {

            }

            return null;
        }

        public bool SetReadMeContents()
        {





            return false;
        }
    }
}
