if (-not (Get-ComputerRestorePoint)) {
  Enable-ComputerRestore -Drive $Env:SystemDrive
}

Checkpoint-Computer -Description \"System Restore Point created by WinUtil\" -RestorePointType MODIFY_SETTINGS
Write-Host \"System Restore Point Created Successfully\" -ForegroundColor Green
