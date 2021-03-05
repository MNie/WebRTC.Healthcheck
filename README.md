# WebRTC.Healthcheck
Small utility to check if WebRTC servers are alive

How to run

1. Set env variable `WEBRTC_SECRET` if you gonna check turn servers with time limited credentials
2. Run by `dotnet run -- "stun.l.google.com:19302"`


Things yet to cover:
1. Parsing turn server with long-living credentials,
2. Write information how to integrate it with Bamboo/Slack/Azure/AWS,
3. Wrap everything in Expecto test lib.
