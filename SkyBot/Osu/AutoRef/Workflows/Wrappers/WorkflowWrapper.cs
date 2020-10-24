using System;
using System.Collections.Generic;
using System.Linq;
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
        public int TotalSteps => _steps.Count;

        int _index;
        List<Func<bool>> _steps;

        internal WorkflowWrapper()
        {
            _steps = new List<Func<bool>>();
        }

        public void AddStep(Func<bool> step)
        {
            _steps.Add(step);
        }

        public void InsertStep(Func<bool> step, int index)
        {
            if (_steps.Count == 0)
                AddStep(step);
            else if (index >= _steps.Count)
                index = _steps.Count - 1;
            else if (index < 0)
                index = 0;

            _steps.Insert(index, step);
        }

        public void AddSteps(IEnumerable<Func<bool>> steps)
        {
            _steps.AddRange(steps);
        }

        public void AddSteps(ReadOnlySpan<Func<bool>> steps)
        {
            for (int i = 0; i < steps.Length; i++)
                AddStep(steps[i]);
        }

        public void ClearSteps()
        {
            _steps.Clear();
            _index = 0;
        }

        public Func<bool> GetNextStep()
        {
            if (_steps.Count == 0 || _steps.Count - 1 == _index)
                return null;

            _index++;
            return GetCurrentStep();
        }

        public Func<bool> GetFirstStep()
        {
            if (_steps.Count == 0)
                return null;

            return _steps[0];
        }

        public Func<bool> GetLastStep()
        {
            if (_steps.Count == 0)
                return null;

            return _steps[_steps.Count - 1];
        }

        public Func<bool> GetCurrentStep()
        {
            if (_steps.Count == 0)
                return null;

            return _steps[_index];
        }

        public Func<bool> GetStep(int index)
        {
            if (_steps.Count == 0)
                return null;

            if (index < 0)
                return _steps[0];
            else if (index >= _steps.Count)
                return _steps[_steps.Count - 1];

            return _steps[index];
        }

        public List<Func<bool>> GetAllSteps()
        {
            return _steps.ToList();
        }
    }
}
