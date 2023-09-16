using System.Text;

namespace KyoFileSignerCLI.Http
{
    // public interface Result<T>
    // {
    //     Expression<Func<T, string, dynamic>> Exp { get; }
    // }

    // public class LoginResponse : Result<Login>
    // {
    //     public Expression<Func<Login, string, dynamic>> Exp => (x, y) => x.Token;
    // }

    public interface IResponse
    {
        public bool ErrorExists();
        public void PrintErrors();
        public string GetErrors();
        public class Only<T> : IResponse
        {
            public int StatusCode { get; set; }
            public bool IsSuccess { get; set; }
            public List<string> ErrorMessages { get; set; }
            public T Result { get; set; }

            public bool ErrorExists()
            {
                return ErrorMessages.Count > 0;
            }

            // public Dictionary<string, dynamic> Result { get; set; }
            // public Result Result { get; set; }
            public void PrintErrors()
            {
                foreach (var error in ErrorMessages)
                {
                    Console.WriteLine(error);
                }
            }

            public string GetErrors()
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var error in ErrorMessages)
                {
                    stringBuilder.AppendLine(error);
                }
                return stringBuilder.ToString();
            }
        }
        public class Many<T> : List<Only<T>>, IResponse
        {
            public bool ErrorExists()
            {
                var errorExists = false;

                ForEach((response) =>
                {
                    if (response.ErrorExists())
                    {
                        errorExists = true;
                        return;
                    }
                });

                return errorExists;
            }

            public void PrintErrors()
            {
                ForEach((response) => response.PrintErrors());
            }

            public List<T> Items()
            {
                List<T> results = new();
                ForEach((response) => results.Add(response.Result));
                return results;
            }

            public string GetErrors()
            {
                StringBuilder stringBuilder = new StringBuilder();
                ForEach((response) => stringBuilder.AppendLine(response.GetErrors()));
                return stringBuilder.ToString();
            }

            // How does this still works?
            // public List<T> Results
            // {
            //     get
            //     {
            //         List<T> results = new();
            //         ForEach((response) => results.Add(response.Result));
            //         return results;
            //     }
            // }
        }
    }
}
