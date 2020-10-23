using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflows.Wrappers
{
    public class WorkflowWrapper
    {
        public int Index
        {
            get => _index;
            set => _index = Math.Max(0, Math.Min(_steps.Count - 1, value));
        }

        int _index;
        List<Action> _steps;

        internal WorkflowWrapper()
        {
            _steps = new List<Action>();
        }

        public void AddStep(Action step)
        {
            _steps.Add(step);
        }

        public void AddSteps(IEnumerable<Action> steps)
        {
            _steps.AddRange(steps);
        }

        public void AddSteps(ReadOnlySpan<Action> steps)
        {
            for (int i = 0; i < steps.Length; i++)
                AddStep(steps[i]);
        }

        public void ClearSteps()
        {
            _steps.Clear();
        }

        public Action GetNextStep()
        {
            if (_steps.Count == 0 || _steps.Count - 1 == _index)
                return null;

            _index++;
            return GetCurrentStep();
        }

        public Action GetFirstStep()
        {
            if (_steps.Count == 0)
                return null;

            return _steps[0];
        }

        public Action GetLastStep()
        {
            if (_steps.Count == 0)
                return null;

            return _steps[_steps.Count - 1];
        }

        public Action GetCurrentStep()
        {
            if (_steps.Count == 0)
                return null;

            return _steps[_index];
        }
    }
}
