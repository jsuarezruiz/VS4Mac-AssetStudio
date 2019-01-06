using System;

namespace VS4Mac.AssetStudio.Exceptions
{
    public class TinyPngApiException : Exception
    {
        public string ErrorTitle { get; }
        public string ErrorMessage { get; }


        public TinyPngApiException(string errorTitle, string errorMessage)
        {
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;

            Data.Add(nameof(ErrorTitle), ErrorTitle);
            Data.Add(nameof(ErrorMessage), ErrorMessage);
        }

        public override string Message =>
            $"TinyPNG Api returned an error when attempting an operation on an image: {ErrorTitle}, {ErrorMessage}";
    }
}