namespace WebRTC.Healthcheck

module PeerConnection =

    open Utils
    open ConnectionState
    open SIPSorcery.Net
    open System
    
    let private candidateHandler log forcedProtocol (state: State): Action<RTCIceCandidate> =
        Action<RTCIceCandidate>(
            fun (candidate: RTCIceCandidate) ->
                state.Add candidate.protocol candidate.``type`` forcedProtocol)
        
    let connect log server (state: State) =
        let iceServer = Server.asIceServer server
        let forcedProtocol = Server.getForcedProtocol server
        let candDel = candidateHandler log forcedProtocol state
        let config = RTCConfiguration (iceServers = asList [ iceServer ])
        use pc = new RTCPeerConnection (config)

        pc.add_onicecandidate candDel
        async {
            let! dc = pc.createDataChannel ("test", RTCDataChannelInit()) |> Async.AwaitTask
            let offerResult = pc.createOffer (RTCOfferOptions ())
            log.Success $"Offer for: %A{server} created: {offerResult.sdp}, data channel: {dc.label}"
            
            let! result = state.WaitForResult ()
                
            pc.remove_onicecandidate candDel
            
            return
                iceServer.urls,
                result
        }

