#include<stdio.h>
FILE *inp, *out;
void main()
{
	unsigned char inbuf[143];
	int i;

	inp=fopen("outputFile.bin","r+b");
	out=fopen("time.txt","w");
	while(!feof(inp)){
	fread(inbuf,sizeof(unsigned char),143,inp);
	for(i=0;i<143;i++){
	//	printf("%02X  ",inbuf[i]);
		fprintf(out,"%02X  ",inbuf[i]);
	}
//	printf("\n");
	fprintf(out,"\n");
	}

}
