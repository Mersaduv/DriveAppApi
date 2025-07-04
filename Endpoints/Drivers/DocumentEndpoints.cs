using DriveApp.DTOs.Drivers;
using DriveApp.Enums;
using DriveApp.Services.Drivers;
using Microsoft.AspNetCore.Http;

namespace DriveApp.Endpoints.Drivers;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/documents").WithTags("Driver Documents");

        // Get all documents
        group.MapGet("/", async (IDocumentService documentService) =>
        {
            var documents = await documentService.GetAllDocumentsAsync();
            return Results.Ok(documents);
        })
        .WithName("GetAllDocuments")
        .WithOpenApi();

        // Get document by ID
        group.MapGet("/{id}", async (Guid id, IDocumentService documentService) =>
        {
            try
            {
                var document = await documentService.GetDocumentByIdAsync(id);
                return Results.Ok(document);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetDocumentById")
        .WithOpenApi();

        // Get documents by driver ID
        group.MapGet("/driver/{driverId}", async (Guid driverId, IDocumentService documentService) =>
        {
            var documents = await documentService.GetDocumentsByDriverIdAsync(driverId);
            return Results.Ok(documents);
        })
        .WithName("GetDocumentsByDriverId")
        .WithOpenApi();

        // Get documents by vehicle ID
        group.MapGet("/vehicle/{vehicleId}", async (Guid vehicleId, IDocumentService documentService) =>
        {
            var documents = await documentService.GetDocumentsByVehicleIdAsync(vehicleId);
            return Results.Ok(documents);
        })
        .WithName("GetDocumentsByVehicleId")
        .WithOpenApi();

        // Upload a document
        group.MapPost("/upload", async (
            IFormFile file,
            DocumentType documentType,
            Guid? driverId, 
            Guid? vehicleId, 
            IDocumentService documentService) =>
        {
            try
            {
                var uploadDto = new UploadDocumentDto
                {
                    DocumentType = documentType,
                    DriverId = driverId,
                    VehicleId = vehicleId
                };
                
                var result = await documentService.UploadDocumentAsync(file, uploadDto);
                return Results.Created($"/api/documents/{result.Id}", result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        })
        .WithName("UploadDocument")
        .WithOpenApi()
        .DisableAntiforgery();

        // Create a document (simplified for form-based submissions)
        group.MapPost("/", async (HttpRequest request, IDocumentService documentService) =>
        {
            try
            {
                if (!request.HasFormContentType)
                {
                    return Results.BadRequest(new { Error = "Content type must be multipart/form-data" });
                }

                var form = await request.ReadFormAsync();
                var file = form.Files["file"];
                
                if (file == null)
                {
                    return Results.BadRequest(new { Error = "File is required" });
                }

                if (!Enum.TryParse<DocumentType>(form["documentType"], out var documentType))
                {
                    return Results.BadRequest(new { Error = "Invalid document type" });
                }

                Guid? driverId = null;
                if (Guid.TryParse(form["driverId"], out var parsedDriverId))
                {
                    driverId = parsedDriverId;
                }

                Guid? vehicleId = null;
                if (Guid.TryParse(form["vehicleId"], out var parsedVehicleId))
                {
                    vehicleId = parsedVehicleId;
                }

                var createDto = new CreateDocumentDto
                {
                    DocumentType = documentType,
                    DriverId = driverId,
                    VehicleId = vehicleId,
                    File = file
                };
                
                var result = await documentService.CreateDocumentAsync(createDto);
                return Results.Created($"/api/documents/{result.Id}", result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        })
        .WithName("CreateDocument")
        .WithOpenApi()
        .DisableAntiforgery();

        // Update document
        group.MapPut("/{id}", async (Guid id, UpdateDocumentDto documentDto, IDocumentService documentService) =>
        {
            try
            {
                var result = await documentService.UpdateDocumentAsync(id, documentDto);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateDocument")
        .WithOpenApi();

        // Delete document
        group.MapDelete("/{id}", async (Guid id, IDocumentService documentService) =>
        {
            var result = await documentService.DeleteDocumentAsync(id);
            if (!result)
                return Results.NotFound();

            return Results.NoContent();
        })
        .WithName("DeleteDocument")
        .WithOpenApi();

        // Verify document
        group.MapPatch("/{id}/verify", async (Guid id, IDocumentService documentService) =>
        {
            var result = await documentService.VerifyDocumentAsync(id);
            if (!result)
                return Results.NotFound();

            return Results.Ok(new { Message = "Document verified successfully" });
        })
        .WithName("VerifyDocument")
        .WithOpenApi();

        // Reject document
        group.MapPatch("/{id}/reject", async (Guid id, string reason, IDocumentService documentService) =>
        {
            var result = await documentService.RejectDocumentAsync(id, reason);
            if (!result)
                return Results.NotFound();

            return Results.Ok(new { Message = "Document rejected successfully" });
        })
        .WithName("RejectDocument")
        .WithOpenApi();
        
        // Verify document with details
        group.MapPatch("/{id}/verify-with-details", async (
            Guid id, 
            DocumentVerificationDto verificationDto, 
            IDocumentService documentService) =>
        {
            try
            {
                var result = await documentService.VerifyDocumentAsync(id, verificationDto);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("VerifyDocumentWithDetails")
        .WithOpenApi();
    }
} 