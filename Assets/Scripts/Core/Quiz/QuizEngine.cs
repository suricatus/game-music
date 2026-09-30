using System;
using System.Collections.Generic;
using Eco.Core.Matching;
using Eco.Core.Models;

namespace Eco.Core.Quiz
{
    public class AnswerOutcome
    {
        public readonly QuestionDef Question;
        public readonly OptionDef ChosenOption;
        public readonly IReadOnlyDictionary<string, double> AxisChanges;
        public readonly IReadOnlyDictionary<string, double> CurrentVibe;
        public readonly bool IsComplete;

        public AnswerOutcome(QuestionDef question, OptionDef chosenOption, IReadOnlyDictionary<string, double> axisChanges,
            IReadOnlyDictionary<string, double> currentVibe, bool isComplete)
        {
            Question = question;
            ChosenOption = chosenOption;
            AxisChanges = axisChanges;
            CurrentVibe = currentVibe;
            IsComplete = isComplete;
        }
    }

    public class QuizEngine
    {
        private readonly Campaign _campaign;
        private readonly ProfileVector _profile;
        private int _currentIndex;

        public QuizEngine(Campaign campaign)
        {
            _campaign = campaign;
            _profile = new ProfileVector(campaign.Axes, campaign.Scoring.AxisFloor);
        }

        public int QuestionNumber => _currentIndex + 1;
        public int TotalQuestions => _campaign.Questions.Count;
        public bool IsComplete => _currentIndex >= _campaign.Questions.Count;
        public QuestionDef CurrentQuestion => IsComplete ? null : _campaign.Questions[_currentIndex];
        public IReadOnlyDictionary<string, double> CurrentVibe => _profile.Values;

        public AnswerOutcome Answer(int optionIndex)
        {
            if (IsComplete)
                throw new InvalidOperationException("Quiz already complete — there are no more questions to answer.");

            var question = CurrentQuestion;
            if (optionIndex < 0 || optionIndex >= question.Options.Count)
                throw new ArgumentOutOfRangeException(nameof(optionIndex),
                    $"Question '{question.Id}' has {question.Options.Count} options; index {optionIndex} is out of range.");

            var option = question.Options[optionIndex];
            var axisChanges = _profile.ApplyAnswer(question, option, _campaign.Scoring);
            _currentIndex++;

            return new AnswerOutcome(question, option, axisChanges,
                new Dictionary<string, double>(_profile.Values), IsComplete);
        }

        public List<TrackMatch> ComputeResult()
        {
            if (!IsComplete)
                throw new InvalidOperationException("Cannot compute a result before every question has been answered.");

            return MatchCalculator.Rank(_profile, _campaign.Axes, _campaign.Tracks, _campaign.Scoring);
        }
    }
}
