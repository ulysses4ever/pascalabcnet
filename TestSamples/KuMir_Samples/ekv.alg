��� GraphABC

��� mw
mw:=2.9

��� �������_�����
���
  SetWindowCaption("��������: �-�������� �� ������");
  SetWindowSize(750,530);
  SetPenColor(clWhite);
  SetBrushColor(clRandom);
  �_�������(4,125,18,490)
���

��� �_�������(��� n,x,y,w);
���
  ��� w1,h
  w1:=int(w/mw)
  h:=div((w-2*w1),3)
  FillRectangle(x,y,x+w,y+w)
  ���� n>0 ��
    Sleep(10)
    SetBrushColor(clRandom)
    �_�������(n-1,x+h,y+h,w1)
    �_�������(n-1,x+w-h-w1,y+h,w1)
    �_�������(n-1,x+h,y+w-h-w1,w1)
    �_�������(n-1,x+w-h-w1,y+w-h-w1,w1)
  ���
���