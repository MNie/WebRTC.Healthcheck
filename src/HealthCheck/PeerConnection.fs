namespace WebRTC.Healthcheck
module PeerConnection =

    open Microsoft.MixedReality.WebRTC
    open Utils
    open ConnectionState

    let private candidateHandler log forcedProtocol (state: State) (candidate: IceCandidate) =
        let parsedCandidate = Candidates.parse log candidate.Content
        match parsedCandidate with
        | Some pro, Some cand ->
            state.Add pro cand forcedProtocol
        | _, _ ->
            log.Failure $"Candidates couldn't be fully parsed for: %s{candidate.Content} and result in: %A{parsedCandidate}"
        ()
        
    let private candidateDelegate log forcedProtocol state = PeerConnection.IceCandidateReadytoSendDelegate (candidateHandler log forcedProtocol state)

    let connect log server (state: State) =
        use pc = new PeerConnection ()
        let forcedProtocol = Server.getForcedProtocol server
        let candDel = candidateDelegate log forcedProtocol state
        pc.add_IceCandidateReadytoSend candDel
        
        let iceServer = Server.asIceServer server
        let config = PeerConnectionConfiguration(IceServers = asList [ iceServer ])
        pc.InitializeAsync(config)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        let dc = pc.AddDataChannelAsync("test", true, true) |> Async.AwaitTask |> Async.RunSynchronously
        
        let offerResult = pc.CreateOffer ()
        log.Success $"Offer for: %A{server} created: {offerResult}, data channel: {dc.Label}"
        
        let result = state.WaitForResult ()
            
        pc.remove_IceCandidateReadytoSend candDel
        
        iceServer.Urls.Find (fun _ -> true),
        result

