// <copyright file="IremoveDB.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PROXY_TODB.DBInterfaces
{
    public interface IremoveDB
    {
        public Task<string> Delete(string hash);
    }
}
