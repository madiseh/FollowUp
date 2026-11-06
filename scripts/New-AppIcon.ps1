# Rebuild the native Windows ICO and PNG from the same geometry as Assets/FollowUp.svg.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assetDir = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\FollowUp\Assets'))
function Round-Path([float]$x,[float]$y,[float]$w,[float]$h,[float]$r) {
    $p = New-Object Drawing.Drawing2D.GraphicsPath
    $d = 2*$r
    $p.AddArc($x,$y,$d,$d,180,90); $p.AddArc(($x+$w-$d),$y,$d,$d,270,90)
    $p.AddArc(($x+$w-$d),($y+$h-$d),$d,$d,0,90); $p.AddArc($x,($y+$h-$d),$d,$d,90,90); $p.CloseFigure()
    return $p
}
function Brush([string]$hex) { return New-Object Drawing.SolidBrush ([Drawing.ColorTranslator]::FromHtml($hex)) }
function Stroke($g,[string]$color,[float]$width,[float[]]$xy) {
    $p = New-Object Drawing.Pen ([Drawing.ColorTranslator]::FromHtml($color)),$width
    $p.StartCap='Round'; $p.EndCap='Round'; $p.LineJoin='Round'
    $points = for($i=0;$i -lt $xy.Length;$i+=2) { New-Object Drawing.PointF $xy[$i],$xy[$i+1] }
    $g.DrawLines($p,[Drawing.PointF[]]$points); $p.Dispose()
}
$frames = @()
foreach($size in @(16,24,32,48,64,128,256)) {
    $bitmap = New-Object Drawing.Bitmap $size,$size
    $g = [Drawing.Graphics]::FromImage($bitmap); $g.SmoothingMode='AntiAlias'; $g.PixelOffsetMode='HighQuality'; $g.ScaleTransform(($size/256.0),($size/256.0))
    $teal=Brush '#0F766E'; $light=Brush '#13877D'; $white=Brush '#F8FAFC'; $mint=Brush '#99D9CD'; $amber=Brush '#FBBF24'
    $shape=Round-Path 8 8 240 240 56; $g.FillPath($teal,$shape); $shape.Dispose()
    $shape=Round-Path 8 8 240 112 56; $g.FillPath($light,$shape); $shape.Dispose()
    $shape=New-Object Drawing.Drawing2D.GraphicsPath
    $shape.AddLines([Drawing.PointF[]]@([Drawing.PointF]::new(70,48),[Drawing.PointF]::new(142,48),[Drawing.PointF]::new(180,86),[Drawing.PointF]::new(180,196),[Drawing.PointF]::new(70,196)))
    $shape.AddArc(58,172,24,24,90,90); $shape.AddLine(58,184,58,60); $shape.AddArc(58,48,24,24,180,90); $shape.CloseFigure(); $g.FillPath($white,$shape); $shape.Dispose()
    $g.FillPolygon($mint,[Drawing.PointF[]]@([Drawing.PointF]::new(142,48),[Drawing.PointF]::new(142,86),[Drawing.PointF]::new(180,86)))
    Stroke $g '#0F766E' 12 @(81,117,95,131,124,100)
    Stroke $g '#B7D4D1' 9 @(83,158,132,158); Stroke $g '#B7D4D1' 9 @(83,176,115,176)
    $g.FillEllipse($teal,132,129,94,94); $g.FillEllipse($amber,141,138,76,76); Stroke $g '#173F49' 9 @(179,153,179,177,195,187)
    $stream=New-Object IO.MemoryStream; $bitmap.Save($stream,[Drawing.Imaging.ImageFormat]::Png)
    $frames+=,@{Size=$size;Bytes=$stream.ToArray()}
    if($size -eq 256) { [IO.File]::WriteAllBytes((Join-Path $assetDir 'FollowUp.png'),$stream.ToArray()) }
    $stream.Dispose(); $g.Dispose(); $bitmap.Dispose(); $teal.Dispose(); $light.Dispose(); $white.Dispose(); $mint.Dispose(); $amber.Dispose()
}
$file=[IO.File]::Create((Join-Path $assetDir 'FollowUp.ico')); $writer=New-Object IO.BinaryWriter $file
$writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$frames.Count)
$offset=6+16*$frames.Count
foreach($frame in $frames) {
    $dimension=if($frame.Size -eq 256){0}else{$frame.Size}
    $writer.Write([byte]$dimension); $writer.Write([byte]$dimension); $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$frame.Bytes.Length); $writer.Write([uint32]$offset); $offset+=$frame.Bytes.Length
}
foreach($frame in $frames) { $writer.Write([byte[]]$frame.Bytes) }
$writer.Dispose(); $file.Dispose()
Write-Output 'Generated FollowUp.ico (7 sizes) and FollowUp.png.'
