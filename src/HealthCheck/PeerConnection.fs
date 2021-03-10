module PeerConnection

open Microsoft.MixedReality.WebRTC
open Utils
open ConnectionState

let private candidateHandler forcedProtocol (state: State) (candidate: IceCandidate) =
    let parsedCandidate = Candidates.parse candidate.Content
    match parsedCandidate with
    | Some pro, Some cand ->
        state.Add pro cand forcedProtocol
    | _, _ ->
        Log.failure $"Candidates couldn't be fully parsed for: %s{candidate.Content} and result in: %A{parsedCandidate}"
    ()
    
let private candidateDelegate forcedProtocol state = PeerConnection.IceCandidateReadytoSendDelegate (candidateHandler forcedProtocol state)

let connect server (state: State) =
    use pc = new PeerConnection ()
    let forcedProtocol = Server.getForcedProtocol server
    pc.add_IceCandidateReadytoSend (candidateDelegate forcedProtocol state)
    
    let iceServer = Server.asIceServer server
    let config = PeerConnectionConfiguration(IceServers = asList [ iceServer ])
    pc.InitializeAsync(config)
    |> Async.AwaitTask
    |> Async.RunSynchronously
    let dc = pc.AddDataChannelAsync("test", true, true) |> Async.AwaitTask |> Async.RunSynchronously
    
    let offerResult = pc.CreateOffer ()
    Log.info $"Offer for: %A{server} created: {offerResult}, data channel: {dc.Label}"
    
    let result = state.WaitForResult ()
        
    pc.remove_IceCandidateReadytoSend (candidateDelegate forcedProtocol state)
    
    result

