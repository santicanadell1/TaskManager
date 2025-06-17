using Domain.Exceptions.TaskExceptions;

namespace Domain;

public class Task
{
    public string _description;
    public int _duration;
    public List<Task> _previousTasks;
    public List<Resource> _resources = new();
    public List<Task> _sameTimeTasks;
    public State _state;
    public string _title;

    public Task()
    {
    }

    public Task(string title, string description, DateTime startDate, int duration, List<Task> previousTasks,
        List<Task> sameTimeTasks, List<Resource> resources)
    {
        Title = title;
        Description = description;
        ExpectedStartDate = startDate;
        Duration = duration;
        PreviousTasks = previousTasks ?? new List<Task>();
        SameTimeTasks = sameTimeTasks ?? new List<Task>();
        State = State.TODO;
        Resources = resources ?? new List<Resource>();

        StartDate = startDate;
        EndDate = startDate.AddDays(duration);
        LatestStart = startDate;
        LatestFinish = startDate.AddDays(duration);
        Slack = TimeSpan.Zero;
        IsCritical = false;
    }

    public int? Id { get; set; }
    public DateTime ExpectedStartDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime LatestStart { get; set; }
    public DateTime LatestFinish { get; set; }
    public TimeSpan Slack { get; set; }
    public bool IsCritical { get; set; }
    public State State { get; set; }

    public List<Resource> Resources
    {
        get => _resources;
        set
        {
            if (value == null) throw new TaskResourceException("Resources cannot be null ");

            _resources = value;
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new TaskTitleException();

            _title = value;
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new TaskDescriptionException();

            _description = value;
        }
    }

    public int Duration
    {
        get => _duration;
        set
        {
            if (value <= 0) throw new TaskDurationException();

            _duration = value;
        }
    }

    public List<Task> PreviousTasks
    {
        get => _previousTasks;
        set
        {
            if (value == null) throw new TaskPreviousTaskException("PreviousTasks cannot be null ");

            _previousTasks = value;
        }
    }

    public List<Task> SameTimeTasks
    {
        get => _sameTimeTasks;
        set => _sameTimeTasks = value ?? new List<Task>();
    }

    public void AddPreviousTask(Task task)
    {
        if (task == null) throw new TaskResourceException("Task cannot be null.");

        PreviousTasks.Add(task);
    }

    public void RemovePreviousTask(Task task)
    {
        if (task == null) throw new TaskResourceException("Task cannot be null.");

        PreviousTasks.Remove(task);
    }

    public void AddSameTimeTask(Task task)
    {
        if (task == null) throw new TaskResourceException("Task cannot be null.");

        SameTimeTasks.Add(task);
    }

    public void RemoveSameTimeTask(Task task)
    {
        if (task == null) throw new TaskResourceException("Task cannot be null.");

        SameTimeTasks.Remove(task);
    }
}