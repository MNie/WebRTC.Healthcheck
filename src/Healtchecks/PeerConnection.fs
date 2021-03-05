module PeerConnection

open Microsoft.MixedReality.WebRTC
open Utils

let private candidateHandler forcedProtocol (candidate: IceCandidate) =
    let parsedCandidate = Candidates.parse candidate.Content
    match parsedCandidate with
    | Some pro, Some cand ->
        State.add pro cand forcedProtocol
    | _, _ ->
        Log.failure $"Candidates couldn't be fully parsed for: %s{candidate.Content} and result in: %A{parsedCandidate}"
    ()
    
let private candidateDelegate forcedProtocol = PeerConnection.IceCandidateReadytoSendDelegate (candidateHandler forcedProtocol)

// check if server is stun when no credentials are given
let connect server =
    use pc = new PeerConnection ()
    let forcedProtocol = Server.getForcedProtocol server
    pc.add_IceCandidateReadytoSend (candidateDelegate forcedProtocol)
    
    let iceServer = Server.asIceServer server
    let config = PeerConnectionConfiguration(IceServers = asList [ iceServer ])
    pc.InitializeAsync(config)
    |> Async.AwaitTask
    |> Async.RunSynchronously
    let dc = pc.AddDataChannelAsync("test", true, true) |> Async.AwaitTask |> Async.RunSynchronously
    
    let offerResult = pc.CreateOffer ()
    
    Log.info $"Offer for: %A{server} created: {offerResult}, data channel: {dc.Label}"
    
    State.waitTillCandidatesWouldBeCreated ()
        
    pc.remove_IceCandidateReadytoSend (candidateDelegate forcedProtocol)

