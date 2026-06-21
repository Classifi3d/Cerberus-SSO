namespace Infrastructure.Resilience.ChaosEngineering;
public class ChaosEngineeringOptions
{
    public bool Enabled { get; set; }
    public ChaosInboundOptions Inbound { get; set; } = new();
    public ChaosOutboundOptions Outbound { get; set; } = new(); 

}

public class ChaosInboundOptions
{
    public bool Enabled {  set; get; }
    public bool HTTPHeaderEnabled { set; get; }
    public double LatencyInjectionRate { set; get; }
    public int LatencyDurationMs { set; get; }
    public double FaultInjectionRate { set; get; }
    public string FaultException { set; get; } = string.Empty;
}

public class ChaosOutboundOptions
{
    public bool Enabled { set; get; }
    public bool HTTPHeaderEnabled { set; get; }
    public double LatencyInjectionRate { set; get; }
    public int LatencyDurationMs { set; get; }
    public double FaultInjectionRate { set; get; }
    public string FaultException { set; get; } = string.Empty;
    public double OutcomeInjectionRate { set; get; }
    public int OutcomeHTTPResponse { set; get; }
    public string[] EnabledUrlList { set; get; } = [];
}
