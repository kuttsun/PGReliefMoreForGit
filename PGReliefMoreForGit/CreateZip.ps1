# �v���W�F�N�g���[�g�Ŏ��s���邱��
using namespace System.IO;
using namespace System.IO.Compression;

# source directory path (Release directory)
$sourceDir = $Args[0]
if (-not ($sourceDir)) { return 100 }

# target zip file path
$targetZipFile = $Args[1]
if (-not ($targetZipFile)) { return 101 }

# target zip file ����f�B���N�g�������擾
$parent = [Path]::GetDirectoryName($targetZipFile)
# target zip file ��u���f�B���N�g�����Ȃ��ꍇ�͍쐬����
[Directory]::CreateDirectory($parent)
# ���� zip �t�@�C��������ꍇ�͍폜���Ă���
[File]::Delete($targetZipFile)

# �ꎞ�f�B���N�g����
$tempDir = $parent + "\temp"
# Release �f�B���N�g������ꎞ�f�B���N�g���Ɋۂ��ƃR�s�[
Copy-Item $sourceDir -destination $tempDir -recurse

# �ꎞ�f�B���N�g������s�v�ȃt�@�C�����폜(�v���W�F�N�g�ɉ����ėv�ύX)
Remove-Item -Recurse -path $tempDir\lib\linux
Remove-Item -Recurse -path $tempDir\lib\osx
Remove-Item -Recurse -path $tempDir\logs
Remove-Item -Recurse -path $tempDir -include *.pdb
Remove-Item -Recurse -path $tempDir -include *.xml
Remove-Item -Recurse -path $tempDir -include *.config -Exclude NLog.config

# �A�Z���u���̓ǂݍ���
Add-Type -AssemblyName System.IO.Compression.FileSystem

# zip �t�@�C���쐬
[ZipFile]::CreateFromDirectory($tempDir, $targetZipFile)

# �ꎞ�f�B���N�g���폜
Remove-Item -Recurse -path $tempDir

