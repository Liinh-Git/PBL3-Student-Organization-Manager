$response = Invoke-RestMethod -Uri "http://localhost:5058/api/admin/apply-migration" -Method POST
Write-Output $response
