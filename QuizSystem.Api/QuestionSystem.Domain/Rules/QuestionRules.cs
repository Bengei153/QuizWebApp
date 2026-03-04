using System;
using QuizSystem.Api.QuestionSystem.Domain.Common;
using QuizSystem.Api.QuestionSystem.Domain.Enums;

namespace QuizSystem.Api.QuestionSystem.Domain.Rules;

public static class QuestionRules
{
    public static void ValidateAnswerCount(
        QuestionType type,
        int count)
    {
        if (type == QuestionType.SingleChoice && count != 1)
            throw new DomainException("Single choice requires exactly one answer");

        if (type == QuestionType.Text && count != 1)
            throw new DomainException("Text question requires one answer");
    }
}
