// install required dependencies and run main file

open System
open System.Net
open System.IO
open System.Diagnostics

let paketUrl = "http://github.com/fsprojects/Paket/releases/download/5.184.0/paket.bootstrapper.exe"

let deps = """source https://nuget.org/api/v2

nuget MathNet.Numerics prerelease
nuget MathNet.Numerics.FSharp prerelease
"""

ServicePointManager.SecurityProtocol <- SecurityProtocolType.Tls ||| SecurityProtocolType.Tls12 ||| SecurityProtocolType.Tls11 ||| SecurityProtocolType.Ssl3

let bootstrap () =
    async {
        use client = new WebClient()
        Directory.CreateDirectory(".paket") |> ignore
        File.WriteAllText("paket.dependencies", deps)
        let paketFile = Path.Combine(".paket", "paket.exe")
        let! _ = client.AsyncDownloadFile(Uri(paketUrl), paketFile)
        let proc = Process.Start(paketFile, "install")
        proc.WaitForExit()
        return ()
    }


Async.RunSynchronously (bootstrap ())
