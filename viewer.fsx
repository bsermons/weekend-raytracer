open System
open System.Drawing
open System.IO
open System.Windows.Forms

let loadAndMonitor file (pb: PictureBox) =
    pb.Image <- Image.FromFile(file)

    let showFileHandler evt = fun evt -> pb.Image <- Image.FromFile(evt.FullPath)
    let watcher = new FileSystemWatcher()

    watcher.NotifyFilter <- NotifyFilters.LastWrite
    watcher.Filter <- "*.jpg"
    watcher.Changed.Add(showFileHandler)
    watcher.Created.Add(showFileHandler)
    watcher.EnableRaisingEvents <- true


[<STAThread>]
do
    let form = new Form()
    let pb = new PictureBox()
    pb.SizeMode <- PictureBoxSizeMode.AutoSize
    form.Controls.Add(pb)
    loadAndMonitor "output.jpg" pb
    Application.Run(form)
