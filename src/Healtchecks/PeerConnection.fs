module PeerConnection

open Microsoft.MixedReality.WebRTC
open Utils

let private candidateHandler (candidate: IceCandidate) =
    let parsedCandidate = Candidates.parse candidate.Content
    match parsedCandidate with
    | Some pro, Some cand ->
        State.add pro cand
    | _, _ ->
        Log.failure $"Candidates couldn't be fully parsed for: %s{candidate.Content} and result in: %A{parsedCandidate}"
    ()
    
let private candidateDelegate = PeerConnection.IceCandidateReadytoSendDelegate candidateHandler

// check if server is stun when no credentials are given
let connect server =
    use pc = new PeerConnection ()
    pc.add_IceCandidateReadytoSend candidateDelegate
    
    let iceServer = Server.asIceServer server
    let config = PeerConnectionConfiguration(IceServers = asList [ iceServer ])
    pc.InitializeAsync(config)
    |> Async.AwaitTask
    |> Async.RunSynchronously
    let dc = pc.AddDataChannelAsync("test", true, true) |> Async.AwaitTask |> Async.RunSynchronously
    
    let offerResult = pc.CreateOffer ()
    
    Log.info $"Offer for: %A{server} created: {offerResult}, data channel: {dc.Label}"
    
    State.waitTillCandidatesWouldBeCreated ()
        
    pc.remove_IceCandidateReadytoSend candidateDelegate

