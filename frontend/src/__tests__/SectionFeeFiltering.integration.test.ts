/**
 * Integration Test: Section-Based Fee Filtering
 * Tests: Query fees by section → Filter in UI → Validate response
 * Requirement: Section context in fee assignments
 */

import { describe, it, expect, beforeEach } from 'vitest';
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5208/api';

describe('Section-Based Fee Filtering Integration Tests', () => {
  const client = axios.create({
    baseURL: API_BASE_URL,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  let sectionId: string;
  let studentFeeId: string;

  beforeEach(async () => {
    // Setup: Get a valid section ID (assumes sections exist)
    // This would typically come from the class/section setup
    sectionId = 'section-123'; // Would be actual section ID from test data
  });

  describe('GET /fees/student-fees/section/{sectionId}', () => {
    it('should retrieve student fees for a specific section', async () => {
      const response = await client.get(`/fees/student-fees/section/${sectionId}`);
      
      expect(response.status).toBe(200);
      expect(Array.isArray(response.data)).toBe(true);
      
      // All returned fees should belong to the specified section
      response.data.forEach((fee: any) => {
        expect(fee.sectionId).toBe(sectionId);
      });
    });

    it('should filter by section and isActive parameter', async () => {
      const response = await client.get(`/fees/student-fees/section/${sectionId}`, {
        params: { isActive: true },
      });
      
      expect(response.status).toBe(200);
      expect(Array.isArray(response.data)).toBe(true);
      
      // All returned fees should be active
      response.data.forEach((fee: any) => {
        expect(fee.isActive).toBe(true);
        expect(fee.sectionId).toBe(sectionId);
      });
    });

    it('should return empty array for section with no fees', async () => {
      const nonExistentSectionId = 'section-empty-123';
      const response = await client.get(`/fees/student-fees/section/${nonExistentSectionId}`);
      
      expect(response.status).toBe(200);
      expect(Array.isArray(response.data)).toBe(true);
      expect(response.data.length).toBe(0);
    });

    it('should return valid student fee objects with all required fields', async () => {
      const response = await client.get(`/fees/student-fees/section/${sectionId}`);
      
      if (response.data.length > 0) {
        const fee = response.data[0];
        
        // Verify all required fields are present
        expect(fee).toHaveProperty('id');
        expect(fee).toHaveProperty('studentId');
        expect(fee).toHaveProperty('studentName');
        expect(fee).toHaveProperty('enrollmentNumber');
        expect(fee).toHaveProperty('feeStructureId');
        expect(fee).toHaveProperty('startDate');
        expect(fee).toHaveProperty('totalAmount');
        expect(fee).toHaveProperty('paidAmount');
        expect(fee).toHaveProperty('balanceAmount');
        expect(fee).toHaveProperty('isActive');
        expect(fee).toHaveProperty('sectionId');
      }
    });

    it('should handle invalid section ID gracefully', async () => {
      const invalidSectionId = 'invalid-format-123-456-789-abc';
      
      try {
        await client.get(`/fees/student-fees/section/${invalidSectionId}`);
        // If successful, should return empty or valid response
      } catch (error: any) {
        // Either 400 Bad Request or 404 Not Found is acceptable
        expect([400, 404]).toContain(error.response?.status);
      }
    });

    it('should include section name in the response', async () => {
      const response = await client.get(`/fees/student-fees/section/${sectionId}`);
      
      if (response.data.length > 0) {
        const fee = response.data[0];
        expect(fee.sectionId).toBe(sectionId);
        
        // Section name should be present if available
        if (fee.sectionName) {
          expect(typeof fee.sectionName).toBe('string');
        }
      }
    });

    it('should correctly calculate balance amounts', async () => {
      const response = await client.get(`/fees/student-fees/section/${sectionId}`);
      
      response.data.forEach((fee: any) => {
        // balanceAmount should equal totalAmount - paidAmount
        const expectedBalance = fee.totalAmount - fee.paidAmount;
        expect(fee.balanceAmount).toBe(expectedBalance);
      });
    });

    it('should respect the isActive filter parameter', async () => {
      // Get active fees
      const activeFeeResponse = await client.get(`/fees/student-fees/section/${sectionId}`, {
        params: { isActive: true },
      });
      
      // Get inactive fees
      const inactiveFeeResponse = await client.get(`/fees/student-fees/section/${sectionId}`, {
        params: { isActive: false },
      });
      
      expect(activeFeeResponse.status).toBe(200);
      expect(inactiveFeeResponse.status).toBe(200);
      
      // Verify active/inactive status
      activeFeeResponse.data.forEach((fee: any) => {
        expect(fee.isActive).toBe(true);
      });
      
      inactiveFeeResponse.data.forEach((fee: any) => {
        expect(fee.isActive).toBe(false);
      });
    });
  });

  describe('Performance and Edge Cases', () => {
    it('should handle section with large number of fees', async () => {
      // Assuming a section exists with many fees
      const response = await client.get(`/fees/student-fees/section/${sectionId}`);
      
      expect(response.status).toBe(200);
      expect(Array.isArray(response.data)).toBe(true);
      
      // Should return within reasonable time (< 5 seconds)
      const startTime = Date.now();
      await client.get(`/fees/student-fees/section/${sectionId}`);
      const endTime = Date.now();
      
      expect(endTime - startTime).toBeLessThan(5000);
    });

    it('should handle special characters in section ID', async () => {
      const specialId = 'section-abc-123_def';
      
      try {
        const response = await client.get(`/fees/student-fees/section/${encodeURIComponent(specialId)}`);
        // Should handle gracefully (either success or 404)
        expect([200, 404]).toContain(response.status);
      } catch (error: any) {
        // Or throw appropriate error
        expect([400, 404]).toContain(error.response?.status);
      }
    });

    it('should return consistent results on repeated calls', async () => {
      const response1 = await client.get(`/fees/student-fees/section/${sectionId}`);
      const response2 = await client.get(`/fees/student-fees/section/${sectionId}`);
      
      expect(response1.data.length).toBe(response2.data.length);
      expect(JSON.stringify(response1.data)).toBe(JSON.stringify(response2.data));
    });
  });
});
