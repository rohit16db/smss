import { useMutation, useQueryClient } from '@tanstack/react-query';
import { promotionApi, type PromoteStudentsDto } from '../services/api';

export function usePromotion() {
  const queryClient = useQueryClient();

  const promoteBulk = useMutation({
    mutationFn: (data: PromoteStudentsDto) => promotionApi.promoteBulk(data),
    onSuccess: () => {
      // Invalidate relevant queries if needed
      queryClient.invalidateQueries({ queryKey: ['students'] });
      queryClient.invalidateQueries({ queryKey: ['enrollments'] });
    },
  });

  return {
    promoteBulk,
  };
}
