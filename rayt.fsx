open System
open System.Drawing
open System.Drawing.Imaging
open System.Numerics


let runImage (gen: (int -> int -> (int * int * Color) seq)) (nx: int) (ny: int) (filename: string)  =
    use bmp = new Bitmap(nx, ny)
    Seq.iter (fun (x, y, c) -> bmp.SetPixel(x, ny - y - 1, c)) (gen nx ny)
    bmp.Save(filename, ImageFormat.Png)


type Ray(a: Vector3, b: Vector3) =
    member this.Origin = a
    member this.Direction = b
    member this.PointAtParameter (t:float32) = a + t * b


type HitRecord =
    struct
        val mutable t: float32
        val mutable p: Vector3
        val mutable normal: Vector3
    end

[<AbstractClass>]
type IHitable() =
    abstract member Hit: Ray * float32 * float32 * byref<HitRecord> -> bool


type Sphere(center: Vector3, radius: float32) =
    inherit IHitable()
    override this.Hit(ray, tmin, tmax, hitRecord) =
        let oc = ray.Origin - center
        let a = Vector3.Dot(ray.Direction, ray.Direction)
        let b = Vector3.Dot(oc, ray.Direction)
        let c = Vector3.Dot(oc, oc) - radius * radius
        let descriminant = b*b - a*c
        if descriminant > 0.0f then
            let tempp = (-b - float32 (Math.Sqrt(float descriminant))) / a
            if tempp < tmax && tempp > tmin then
                hitRecord.t <- tempp
                hitRecord.p <- ray.PointAtParameter hitRecord.t
                hitRecord.normal <- (hitRecord.p - center) / radius
                true
            else
              let tempm = (-b + float32 (Math.Sqrt(float descriminant))) / a
              if tempm < tmax && tempm > tmin then
                  hitRecord.t <- tempm
                  hitRecord.p <- ray.PointAtParameter hitRecord.t
                  hitRecord.normal <- (hitRecord.p - center) / radius
                  true
              else
                  false
        else
          false


type HitableList (objs: IHitable seq) =
    inherit IHitable()

    override this.Hit(ray, tmin, tmax, hitRecord) =
        let mutable closestSoFar = tmax
        let mutable tempRec = HitRecord()
        let mutable hitAnything = false
        for o in objs do
            if o.Hit(ray, tmin, closestSoFar, ref tempRec) then
                hitAnything <- true
                closestSoFar <- tempRec.t
                hitRecord <- tempRec

        hitAnything



let color (v:Vector3) =
    Color.FromArgb(255, int(255.9f * v.X), int(255.9f * v.Y), int(255.9f * v.Z))


let hitSphere center radius (ray: Ray) =
    let oc = ray.Origin - center
    let a = Vector3.Dot(ray.Direction, ray.Direction)
    let b = 2.0f * Vector3.Dot(oc, ray.Direction)
    let c = Vector3.Dot(oc, oc) - radius * radius
    let descriminant = b*b - 4.0f*a*c
    if descriminant < 0.0f then
        -1.0f
    else
        -b - (float32 (Math.Sqrt(float descriminant))) / 2.0f * a


let rayColor0 (r: Ray) =
    let t = hitSphere (Vector3(0.0f, 0.0f, -1.0f)) 0.5f r
    if t > 0.0f  then
        let N = Vector3.Normalize(r.PointAtParameter(t) - Vector3(0.0f, 0.0f, -1.0f))
        0.5f * Vector3(N.X + 1.0f, N.Y + 1.0f, N.Z + 1.0f)
    else
      let unitDir = Vector3.Normalize(r.Direction)
      let t = 0.5f * (unitDir.Y + 1.0f)
      (1.0f - t) * Vector3(1.0f, 1.0f, 1.0f) + t * Vector3(0.5f, 0.7f, 1.0f)


let genImage0 (nx: int) (ny: int) =
    seq {
      for j in [0 .. ny - 1] do
          for i in [0 .. nx-1] do
              let vec = Vector3 (float32 i / float32 nx, float32 j / float32 ny, 0.2f)
              let ir = int (255.99f * vec.X)
              let ig = int (255.99f * vec.Y)
              let ib = int (255.99f * vec.Z)
              yield (i, j, Color.FromArgb(255, ir, ig, ib)) }


let genImage1 (nx: int) (ny: int) =
    let lowerLeftCorner = Vector3 ( -2.0f, -1.0f, -1.0f )
    let horizontal = Vector3 ( 4.0f, 0.0f, 0.0f )
    let vertical = Vector3 ( 0.0f, 2.0f, 0.0f )
    let origin = Vector3 ( 0.0f, 0.0f, 0.0f )

    seq {
      for j in seq { 0 .. ny - 1 } do
          for i in seq { 0 .. nx - 1 } do
              let u = float32 i / float32 nx
              let  v = float32 j / float32 ny
              let r = Ray(origin, lowerLeftCorner + u * horizontal + v*vertical)
              let col = color (rayColor0 r)
              yield (i, j, col) }


runImage genImage1 200 100 "output1.png"

let rayColor2 (r: Ray) (world: IHitable) =
    let mutable hitRec = HitRecord()
    if world.Hit(r, 0.0f, Single.MaxValue, ref hitRec)  then
        0.5f * Vector3(hitRec.normal.X + 1.0f, hitRec.normal.Y + 1.0f, hitRec.normal.Z + 1.0f)
    else
        let unitDir = Vector3.Normalize(r.Direction)
        let t = 0.5f * (unitDir.Y + 1.0f)
        (1.0f - t) * Vector3(1.0f, 1.0f, 1.0f) + t * Vector3(0.5f, 0.7f, 1.0f)

let genImage2 (nx: int) (ny: int) =
    let lowerLeftCorner = Vector3 ( -2.0f, -1.0f, -1.0f )
    let horizontal = Vector3 ( 4.0f, 0.0f, 0.0f )
    let vertical = Vector3 ( 0.0f, 2.0f, 0.0f )
    let origin = Vector3 ( 0.0f, 0.0f, 0.0f )
    let objs : IHitable[] = [| Sphere(Vector3(0.0f, 0.0f, -1.0f), 0.5f);
                               Sphere(Vector3(0.0f, -100.5f, -1.0f), 100.0f) |]
    let world = HitableList(objs)

    seq {
      for j in seq { 0 .. ny - 1 } do
          for i in seq { 0 .. nx - 1 } do
              let u = float32 i / float32 nx
              let  v = float32 j / float32 ny
              let r = Ray(origin, lowerLeftCorner + u * horizontal + v*vertical)
              let col = color (rayColor2 r world)
              yield (i, j, col) }

runImage genImage2 200 100 "output2.png"
