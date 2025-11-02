namespace TerminateGRInstance.Model;

public class InstanceStatusOutDto
{
    public string Name { get; set; }
    public string InstanceId { get; set; }
    public string RuntimeStatus { get; set; }
    public InputDto Input { get; set; }
    public string CustomStatus { get; set; }
    public string Output { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime LastUpdatedTime { get; set; }
}

public class InputDto
{
    public string ApplicationType { get; set; }
    public string GroupRenewalWorkflowId { get; set; }
    public int NumberOfGroupRenewalTransactions { get; set; }
    public string UserEmail { get; set; }
}