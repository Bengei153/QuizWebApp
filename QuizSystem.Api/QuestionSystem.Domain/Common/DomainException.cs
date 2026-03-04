using System;

namespace QuizSystem.Api.QuestionSystem.Domain.Common;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
