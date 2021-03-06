using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;

namespace AmiLesson.Code.Jobs
{
    public class PreOrderJob : IJob
    {
        public static List<(string User, int LoginId, string Token)> UserWithAccessToken = new();

        public async Task Execute(IJobExecutionContext context)
        {
            var list = BathRoomConfigUtil.GetAllAutoOrderUser();
            UserWithAccessToken.Clear();
            foreach (var item in list)
            {
                var helper = new BathroomOrderHelper();
                await helper.Login(item.user, item.pwd);
                UserWithAccessToken.Add(new(item.user, helper.LoginId, helper.AccessToken));
            }
        }
    }
}