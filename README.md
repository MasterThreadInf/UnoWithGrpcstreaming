# UnoWithGrpcstreaming

Copy proto file in Folder name proto and link to both client and server. 
Add grpc service which will call the method you're interested in.
Build both server and client side so it generates .cs file from proto.
Add client in wasm project reference .
Set multi project to start the project in order to connect each other and use the service. 

Currently client side and bidirectional not working, so feel free to resolve the issue. 
