// <copyright file="IexecScript.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PROXY_TODB.DBInterfaces
{
    public interface IexecScript
    {
        public Task<(string message, bool success)> ExecuteScriptByHash(string hash, string sqlScript);
    }
}
