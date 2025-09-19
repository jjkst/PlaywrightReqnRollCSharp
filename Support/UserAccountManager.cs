using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightMSTests.Support;

public static class UserAccountManager
{
    private static ConcurrentQueue<string> _availableAccounts = [];

    public static void InitializeAccounts(IEnumerable<string> accounts)
    {
        _availableAccounts = new ConcurrentQueue<string>(accounts);
    }

    public static string GetUserAccount()
    {
        _availableAccounts.TryDequeue(out var user);
        return user;
    }

    public static void ReleaseUserAccount(string user)
    {
        if (user != null)
        {
            _availableAccounts.Enqueue(user);
        }
    }
}

